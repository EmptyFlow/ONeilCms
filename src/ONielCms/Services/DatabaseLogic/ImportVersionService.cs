using ONielCms.Models;
using ONielCommon.Entities;
using ONielCommon.Storage;
using SqlKata;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using RouteEntity = ONielCommon.Entities.Route;

namespace ONielCms.Services.DatabaseLogic {

    public class ImportVersionService : IImportVersionService {

        private readonly IStorageContext m_storageContext;

        public ImportVersionService ( IStorageContext storageContext ) => m_storageContext = storageContext;

        private string CalculateHash ( string rawContent ) {
            using var sha256 = SHA256.Create ();
            var hashBytes = sha256.ComputeHash ( Encoding.UTF8.GetBytes ( rawContent ) );
            return BitConverter.ToString ( hashBytes ).Replace ( "-", "" ).ToLowerInvariant ();
        }

        public Task ImportFromFile ( string fileName ) {
            return m_storageContext.MakeInTransaction (
                async () => {
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
                                ContentHash = hash
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
                                        Version = model.Data.Version,
                                    }
                                )
                                .ToList ();
                            foreach ( var routeResource in routeResources ) await m_storageContext.AddOrUpdate ( routeResource );
                        } else {
                            var routeModel = new RouteEntity {
                                ContentType = route.ContentType,
                                Method = route.Method,
                                Path = route.Path,
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
                                        Version = model.Data.Version,
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

    }

}
