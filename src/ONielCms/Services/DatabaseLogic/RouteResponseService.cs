using ONielCommon.Entities;
using ONielCommon.Storage;
using SqlKata;

namespace ONielCms.Services.DatabaseLogic {

    public class RouteResponseService : IRouteResponseService {

        private readonly IStorageContext _storageContext;

        public RouteResponseService ( IStorageContext storageContext ) => _storageContext = storageContext ?? throw new ArgumentNullException ( nameof ( storageContext ) );

        private static async IAsyncEnumerable<Resource> CombineContent ( Stream stream, IEnumerable<Resource> resources ) {
            foreach ( var resource in resources ) {
                await stream.WriteAsync ( resource.Content );
                yield return resource;
            }
        }

        public async Task<(byte[], int)> GetResponse ( string path, Guid routeId, string version, CancellationToken cancellationToken = default ) {
            var resources = await _storageContext.GetAsync<Resource> (
                new Query ()
                    .Join ( "routeresource", "routeresource.resourceid", "resource.id" )
                    .Join ( "resourceversion", "resourceversion.resourceid", "routeresource.resourceid" )
                    .Where ( "routeid", routeId )
                    .Where ( "version", version )
                    .Select ( "resource.*" )
                    .OrderBy ( "routeresource.renderorder" ),
                cancellationToken
            );
            if ( !resources.Any () ) return ([], 204);

            var response = new MemoryStream ();
            await foreach ( var resource in CombineContent ( response, resources ).WithCancellation ( cancellationToken ) ) {
#if DEBUG
                Console.WriteLine ( "Content for resource " + resource.Id );
#endif
            }
            response.Position = 0;

            return (response.ToArray (), 200);
        }

    }

}
