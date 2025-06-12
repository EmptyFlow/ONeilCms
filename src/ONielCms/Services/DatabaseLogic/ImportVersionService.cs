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
            return BitConverter.ToString ( hashBytes ).ToLowerInvariant ();
        }

        public Task ImportFromFile ( string fileName ) {
            return m_storageContext.MakeInTransaction (
                async () => {
                    var content = await File.ReadAllTextAsync ( fileName );
                    ImportVersionModel? model;
                    try {
                        model = JsonSerializer.Deserialize ( content, OnielCmsJsonContext.Default.ImportVersionModel );
                    } catch ( Exception ex ) {
                        throw new Exception ( $"Can't deserialize file at path {fileName}: {ex.Message}" );
                    }
                    if ( model == null ) throw new Exception ( $"Can't deserialize file at path {fileName}!" );

                    var existsVersions = await m_storageContext.GetAsync<Edition> ( new Query () );

                    ValidateModel ( model, existsVersions.Select ( a => a.Version ) );

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
                            resourceIds.Add ( resource.Id, currentHash.Id );
                        } else {
                            var resourceModel = new Resource {
                                Identifier = resource.Id,
                                Content = bytes,
                                ContentHash = hash
                            };
                            await m_storageContext.AddOrUpdate ( resourceModel );

                            resourceIds.Add ( resource.Id, resourceModel.Id );
                        }
                    }

                    foreach ( var route in model.Routes ) {
                        var routeModel = new RouteEntity {
                            ContentType = route.ContentType,
                            Method = route.Method,
                            Path = route.Path,
                        };
                        await m_storageContext.AddOrUpdate ( routeModel );
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
           );
        }

        private static void ValidateModel ( ImportVersionModel model, IEnumerable<string> versions ) {
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
                throw new Exception ( $"Model.Routes.Resources ({route}) have not exists resources - {string.Join ( ", ", notExists )}" );
            }
        }

    }

}
