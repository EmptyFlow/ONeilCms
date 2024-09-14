using ONielCommon.Storage;
using ONielCommon.Storage.EntityServices;
using SqlKata;
using OneilRoute = ONielCommon.Entities.Route;

namespace ONielCms.Services {

    public class RouteService : IRouteService {

        private readonly IStorageContext m_storageContext;

        public RouteService ( IStorageContext storageContext ) => m_storageContext = storageContext ?? throw new ArgumentNullException ( nameof ( storageContext ) );

        public Task<IEnumerable<OneilRoute>> GetRoutes ( Action<Query>? adjust = default ) {
            if ( adjust == null ) return m_storageContext.GetAsync<OneilRoute> ( new Query () );

            var query = new Query ();
            adjust ( query );

            return m_storageContext.GetAsync<OneilRoute> ( query );
        }

        public Task AddOrUpdate ( OneilRoute edition ) => m_storageContext.AddOrUpdate ( edition );

        public Task Delete ( Guid id ) => m_storageContext.Delete<OneilRoute> ( new Query ().Where ( "id", id.ToString () ) );

    }

}
