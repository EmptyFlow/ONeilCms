using ONielCommon.Entities;
using ONielCommon.Storage;
using ONielCommon.Storage.EntityServices;
using SqlKata;
using OneilRoute = ONielCommon.Entities.SiteRoute;

namespace ONielCms.Services {

    public class RouteService : IRouteService {

        private readonly IStorageContext m_storageContext;

        public RouteService ( IStorageContext storageContext ) => m_storageContext = storageContext ?? throw new ArgumentNullException ( nameof ( storageContext ) );

        public async Task<IEnumerable<OneilRoute>> GetAllRoutesInVersion ( string version, string method ) {
            return await m_storageContext.GetAsync<OneilRoute> (
                new Query ()
                    .Join ( "routeversion", "route.id", "routeversion.routeid" )
                    .Where ( "route.method", method )
                    .Where ( "routeversion.version", version )
                    .SelectRaw ( "route.*" )
            );
        }

        public async Task<(IEnumerable<OneilRoute>, string)> GetAllRoutesInCurrentVersion ( string method ) {
            var edition = await m_storageContext.GetSingleAsync<Edition> (
                new Query ()
                    .OrderByDesc ( "created" )
            );
            if ( edition == null ) return (Enumerable.Empty<OneilRoute> (), "");

            var result = await m_storageContext.GetAsync<OneilRoute> (
                new Query ()
                    .Join ( "routeversion", "route.id", "routeversion.routeid" )
                    .Where ( "route.method", method )
                    .Where ( "routeversion.version", edition.Version )
                    .SelectRaw ( "route.*" )
            );

            return (result, edition.Version);
        }

        public Task AddOrUpdate ( OneilRoute edition ) => m_storageContext.AddOrUpdate ( edition );

        public Task Delete ( Guid id ) => m_storageContext.Delete<OneilRoute> ( new Query ().Where ( "id", id.ToString () ) );

    }

}
