using ONielCms.Models;
using ONielCommon.Entities;
using ONielCommon.Storage;
using SqlKata;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using RouteEntity = ONielCommon.Entities.SiteRoute;

namespace ONielCms.Services.DatabaseLogic {

    public class ImportVersionService : IImportVersionService {

        private readonly IStorageContext m_storageContext;

        public ImportVersionService ( IStorageContext storageContext ) => m_storageContext = storageContext;

        private static string CalculateHash ( string rawContent ) => CalculateHash ( Encoding.UTF8.GetBytes ( rawContent ) );

        private static string CalculateHash ( byte[] bytes ) {
            var hashBytes = SHA256.HashData ( bytes );
            return BitConverter.ToString ( hashBytes ).Replace ( "-", "" ).ToLowerInvariant ();
        }

        public Task ImportFromFile ( string fileName ) {
            return m_storageContext.MakeInTransaction (
                async () => {
                    fileName = Path.GetFullPath ( fileName );

                    if ( !File.Exists ( fileName ) ) {
                        Console.WriteLine ( $"File by path: {fileName} not found!" );
                        return;
                    }

                    var content = await File.ReadAllTextAsync ( fileName );
                    if ( content == null ) {
                        Console.WriteLine ( $"Can;t read content from file {fileName}!" );
                        return;
                    }
                    ImportVersionModel? model;
                    try {
                        model = JsonSerializer.Deserialize ( content, OnielCmsJsonContext.Default.ImportVersionModel );
                    } catch ( Exception ex ) {
                        throw new Exception ( $"Can't deserialize file at path {fileName}: {ex.Message}" );
                    }
                    if ( model == null ) throw new Exception ( $"Can't deserialize file at path {fileName}!" );

                    ValidateModel ( model );

                    var version = await m_storageContext.GetSingleAsync<Edition> ( new Query ().Where ( "version", model.Data.Version ) );
                    if ( version != null ) throw new Exception ( $"Version {model.Data.Version} is already imported!" );

                    await m_storageContext.AddOrUpdate (
                        new Edition {
                            Created = DateTime.UtcNow,
                            Version = model.Data.Version,
                        }
                    );

                    var resourcesHashes = ( await m_storageContext.GetAsync<ResourceWithoutContent> ( new Query ().Select ( ["id", "identifier", "contenthash"] ) ) )
                        .ToDictionary ( a => a.Identifier );

                    var resourceIds = new Dictionary<string, Guid> ();
                    foreach ( var resource in model.Resources ) {
                        byte[] bytes = [];
                        string hash = "";
                        if ( resource.RawContent != null ) {
                            bytes = Encoding.UTF8.GetBytes ( resource.RawContent );
                            hash = CalculateHash ( resource.RawContent );
                        }
                        if ( resource.FileContent != null ) {
                            try {
                                bytes = File.ReadAllBytes ( resource.FileContent );
                                hash = CalculateHash ( bytes );
                            } catch ( Exception readFileException ) {
                                throw new Exception ( $"Can't read file {resource.FileContent}: {readFileException.Message}" );
                            }
                        }
                        if ( bytes.Length == 0 ) continue;

                        if ( resourcesHashes.TryGetValue ( resource.Id, out var currentHash ) && hash == currentHash.ContentHash ) {
                            // if resource already exists just need to add link on version
                            resourceIds.Add ( resource.Id, currentHash.Id );

                            var resourceVersion = new ResourceVersion {
                                ResourceId = currentHash.Id,
                                Version = model.Data.Version
                            };
                            await m_storageContext.AddOrUpdate ( resourceVersion );
                        } else {
                            // if resource not exists need to create it
                            var resourceModel = new Resource {
                                Identifier = resource.Id,
                                Content = bytes,
                                ContentHash = hash,
                            };
                            await m_storageContext.AddOrUpdate ( resourceModel );

                            // and link with version
                            var resourceVersion = new ResourceVersion {
                                ResourceId = resourceModel.Id,
                                Version = model.Data.Version
                            };
                            await m_storageContext.AddOrUpdate ( resourceVersion );

                            resourceIds.Add ( resource.Id, resourceModel.Id );
                        }
                    }

                    var existsRoutes = await m_storageContext.GetAsync<RouteEntity> ( new Query () );

                    foreach ( var route in model.Routes ) {
                        var existRoute = existsRoutes
                            .FirstOrDefault ( a => a.Path == route.Path && a.Method == route.Method );
                        if ( existRoute != null ) {
                            var routeVersion = new RouteVersion {
                                RouteId = existRoute.Id,
                                Version = model.Data.Version
                            };
                            await m_storageContext.AddOrUpdate ( routeVersion );

                            var routeResources = route.Resources
                                .Select (
                                    ( a, index ) => new RouteResource {
                                        RenderOrder = index,
                                        ResourceId = resourceIds[a],
                                        RouteId = existRoute.Id,
                                    }
                                )
                                .ToList ();

                            foreach ( var routeResource in routeResources ) await m_storageContext.AddOrUpdate ( routeResource );
                        } else {
                            var routeModel = new RouteEntity {
                                ContentType = route.ContentType,
                                Method = ImportRoutines.GetMethodName ( route.Method ),
                                Path = route.Path,
                                DownloadAsFile = route.DownloadAsFile,
                                DownloadFileName = route.DownloadFileName,
                            };
                            await m_storageContext.AddOrUpdate ( routeModel );
                            var routeVersion = new RouteVersion {
                                RouteId = routeModel.Id,
                                Version = model.Data.Version
                            };
                            await m_storageContext.AddOrUpdate ( routeVersion );

                            var routeResources = route.Resources
                                .Select (
                                    ( a, index ) => new RouteResource {
                                        RenderOrder = index,
                                        ResourceId = resourceIds[a],
                                        RouteId = routeModel.Id,
                                    }
                                )
                                .ToList ();
                            foreach ( var routeResource in routeResources ) await m_storageContext.AddOrUpdate ( routeResource );
                        }
                    }
                }
           );
        }

        private static void ValidateModel ( ImportVersionModel model ) {
            if ( string.IsNullOrEmpty ( model.Data.Version ) ) throw new Exception ( "Model.Data.Version is null of empty!" );
            if ( !model.Routes.Any () && !model.Resources.Any () ) throw new Exception ( "Model.Routes and Model.Resources is empty!" );

            var ids = model.Resources.ToLookup ( a => a.Id, a => a.Id );
            var duplicates = ids
                .Where ( a => a.Count () > 1 )
                .ToList ();
            if ( duplicates.Any () ) throw new Exception ( $"Model.Resources.Id have duplicates - {string.Join ( ", ", duplicates )}" );

            var fileResources = model.Resources
                .Where ( a => !string.IsNullOrEmpty ( a.FileContent ) )
                .Select ( a => a.FileContent!.ToString () )
                .ToList ();
            foreach ( var fileResource in fileResources ) {
                if ( !File.Exists ( Path.GetFullPath ( fileResource ) ) ) throw new Exception ( $"File {fileResource} not found!" );
            }

            var resourceIds = ids
                .SelectMany ( a => a )
                .ToHashSet ();
            foreach ( var route in model.Routes ) {
                var notExists = route.Resources
                    .Where ( a => !resourceIds.Contains ( a ) )
                    .ToList ();
                if ( notExists.Any () ) throw new Exception ( $"Model.Routes.Resources ({route}) have not exists resources - {string.Join ( ", ", notExists )}" );
            }
        }

        /// <summary>
        /// Import version from folder.
        /// </summary>
        /// <param name="path">Path to filder.</param>
        public Task ImportFromFolder ( string path, string desiredVersion ) {
            return m_storageContext.MakeInTransaction (
                async () => {
                    if ( !Path.Exists ( path ) ) {
                        Console.WriteLine ( $"Path {path} not found!" );
                        return;
                    }

                    var directoryInfo = new DirectoryInfo ( path );
                    var files = directoryInfo.GetFiles ( "*", SearchOption.AllDirectories )
                        .ToList ();

                    var version = await m_storageContext.GetSingleAsync<Edition> ( new Query ().Where ( "version", desiredVersion ) );
                    if ( version == null ) {
                        version = new Edition {
                            Created = DateTime.UtcNow,
                            Version = desiredVersion,
                        };
                        await m_storageContext.AddOrUpdate ( version );
                    }

                    var existsRoutes = ( await m_storageContext.GetAsync<RouteEntity> ( new Query ().Where ( "method", "GET" ) ) )
                        .ToDictionary ( a => a.Path );

                    var resourcesHashes = (
                        await m_storageContext.GetAsync<ResourceWithoutContent> (
                            new Query ()
                                .Join ( "resourceversion", "resourceversion.resourceid", "resource.id" )
                                .Select ( ["resource.id", "resource.identifier", "resource.contenthash"] )
                        )
                    ).ToDictionary ( a => a.Identifier );

                    foreach ( var file in files ) {
                        var nameOfResource = file.FullName.Replace ( "\\", "/" ).Substring ( path.Length + 1 );
                        var nameOfRoute = nameOfResource;

                        var bytes = File.ReadAllBytes ( file.FullName );
                        var hash = CalculateHash ( bytes );

                        // if we have existing resource
                        if ( resourcesHashes.TryGetValue ( nameOfResource, out var existsResource ) ) {
                            // and if file hash not equal
                            if ( existsResource.ContentHash != hash ) {
                                // update only resource content and hash
                                await m_storageContext.MakeNoResult<Resource> (
                                    new Query ()
                                        .Where ( "id", existsResource.Id )
                                        .AsUpdate ( new { content = bytes, contenthash = hash } )
                                );
                            }
                            continue;
                        }

                        // if we have route and don't have resource
                        if ( existsRoutes.TryGetValue ( nameOfRoute, out var existRoute ) ) {
                            // create resource 
                            await SaveResource ( nameOfResource, bytes, hash, existRoute.Id, desiredVersion );
                            // and appropriate version if it required
                            await SaveRouteVersion ( existRoute.Id, desiredVersion );
                            continue;
                        }

                        // create new route and all appropriate stuff
                        var mimeType = ImportRoutines.GetMimeTypeForFileExtension ( nameOfResource );
                        var isDownloadble = ImportRoutines.IsDownloadbleFile ( mimeType );
                        var newRoute = new RouteEntity {
                            Method = "GET",
                            ContentType = mimeType,
                            DownloadAsFile = isDownloadble,
                            DownloadFileName = isDownloadble ? Path.GetFileName ( nameOfResource ) : "",
                            Path = nameOfRoute
                        };
                        await m_storageContext.AddOrUpdate ( newRoute );

                        await SaveRouteVersion ( newRoute.Id, desiredVersion );
                        await SaveResource ( nameOfResource, bytes, hash, newRoute.Id, desiredVersion );
                    }
                }
            );
        }

        private async Task SaveRouteVersion ( Guid routeId, string version ) {
            var hasVersion = await m_storageContext.GetSingleAsync<RouteVersion> (
                new Query ().Where ( "version", version ).Where ( "routeid", routeId )
            );
            if ( hasVersion != null ) return;

            await m_storageContext.AddOrUpdate (
                new RouteVersion {
                    RouteId = routeId,
                    Version = version
                }
            );
        }

        private async Task SaveResource ( string nameOfResource, byte[] bytes, string hash, Guid routeId, string version ) {
            var resource = new Resource {
                Identifier = nameOfResource,
                Content = bytes,
                ContentHash = hash,
            };
            await m_storageContext.AddOrUpdate ( resource );

            await m_storageContext.AddOrUpdate (
                new RouteResource {
                    RenderOrder = 0,
                    ResourceId = resource.Id,
                    RouteId = routeId,
                }
            );

            var resourceVersion = new ResourceVersion {
                ResourceId = resource.Id,
                Version = version
            };
            await m_storageContext.AddOrUpdate ( resourceVersion );
        }

    }

}
