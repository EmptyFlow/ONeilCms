using ONielCommon.Entities;
using ONielCommon.Storage;
using SqlKata;
using Route = ONielCommon.Entities.Route;

namespace ONielCms.Services.DatabaseLogic {

    public class RouteManagementService : IRouteManagementService {

        private readonly IStorageContext m_storageContext;

        public RouteManagementService ( IStorageContext storageContext ) => m_storageContext = storageContext;

        private static string GetRouteMethod ( RouteMethod routeMethod ) => routeMethod switch {
            RouteMethod.GET => "GET",
            RouteMethod.POST => "GET",
            RouteMethod.PUT => "PUT",
            RouteMethod.DELETE => "DELETE",
            RouteMethod.PATCH => "PATCH",
            _ => ""
        };

        public Task<Guid> Create ( string path, string contentType, RouteMethod routeMethod ) {
            return m_storageContext.MakeInTransaction (
                async () => {
                    var methodName = GetRouteMethod ( routeMethod );

                    var existsRoutes = await m_storageContext.GetAsync<Route> (
                        new Query ()
                            .Where ( "path", path )
                            .Where ( "method", methodName )
                    );
                    if ( existsRoutes.Any () ) throw new Exception ( $"Route with path {path} and method {methodName} already exists!" );

                    var route = new Route {
                        Path = path,
                        ContentType = contentType,
                        Method = methodName
                    };
                    await m_storageContext.AddOrUpdate ( route );

                    return route.Id;
                }
           );
        }
        public Task Edit ( Guid id, string path, string contentType, RouteMethod routeMethod ) {
            throw new NotImplementedException ();
        }

        public Task AttachResources ( Guid id, IDictionary<Guid, int> resourceIds ) {
            throw new NotImplementedException ();
        }

        public Task DetachResources ( Guid id, IEnumerable<int> resourceIds ) {
            throw new NotImplementedException ();
        }

    }

}
