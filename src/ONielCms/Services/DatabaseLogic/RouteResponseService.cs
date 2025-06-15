using ONielCommon.Entities;
using ONielCommon.Storage;
using SqlKata;
using Route = ONielCommon.Entities.Route;

namespace ONielCms.Services.DatabaseLogic {

    public class RouteResponseService : IRouteResponseService {

        private readonly IStorageContext _storageContext;

        public RouteResponseService ( IStorageContext storageContext ) => _storageContext = storageContext ?? throw new ArgumentNullException ( nameof ( storageContext ) );

        public async Task<(byte[], int)> GetResponse ( string path ) {
            var edition = await _storageContext.GetSingleAsync<Edition> (
                new Query ()
                    .OrderByDesc ( "created", "GET" )
            );
            if ( edition == null ) return ([], 500);

            var route = await _storageContext.GetSingleAsync<Route> (
                new Query ()
                    .Where ( "method", "GET" )
                    .Where ( "path", path.ToLowerInvariant () )
                    .Where ( "version", edition.Version )
            );
            if ( route == null ) return ([], 404);

            var routeResources = await _storageContext.GetAsync<RouteResource> (
                new Query ()
                    .Where ( "routeid", route.Id )
                    .Where ( "version", edition.Version )
                    .OrderBy ( "renderorder" )
            );
            if ( !routeResources.Any () ) return ([], 204);

            var resources = await _storageContext.GetAsync<Resource> (
                new Query ()
                    .Where ( "id", routeResources.Select ( a => a.ResourceId ) )
            );

            var response = new MemoryStream ();
            foreach ( var resource in resources ) {
                await response.WriteAsync ( resource.Content );
            }
            response.Position = 0;

            return (response.ToArray (), 200);
        }

        public async Task<(byte[], int)> GetResponse ( string path, Guid routeId, string version ) {
            var routeResources = await _storageContext.GetAsync<RouteResource> (
            new Query ()
                    .Where ( "routeid", routeId )
                    .Where ( "version", version )
                    .OrderBy ( "renderorder" )
            );
            if ( !routeResources.Any () ) return ([], 204);

            var resources = await _storageContext.GetAsync<Resource> (
                new Query ()
                    .Where ( "id", routeResources.Select ( a => a.ResourceId ) )
            );

            var response = new MemoryStream ();
            foreach ( var resource in resources ) {
                await response.WriteAsync ( resource.Content );
            }
            response.Position = 0;

            return (response.ToArray (), 200);
        }

    }

}
