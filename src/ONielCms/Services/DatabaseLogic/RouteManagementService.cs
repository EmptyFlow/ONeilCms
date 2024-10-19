using ONielCommon.Storage;

namespace ONielCms.Services.DatabaseLogic
{

    public class RouteManagementService : IRouteManagementService {

        private readonly IStorageContext m_storageContext;

        public RouteManagementService(IStorageContext storageContext) => m_storageContext = storageContext;

        public Task AttachResources ( Guid id, IDictionary<Guid, int> resourceIds ) {
            throw new NotImplementedException ();
        }

        public Task<Guid> Create ( string path, string contentType, RouteMethod routeMethod ) {
            throw new NotImplementedException ();
        }

        public Task DetachResources ( Guid id, IEnumerable<int> resourceIds ) {
            throw new NotImplementedException ();
        }

        public Task Edit ( Guid id, string path, string contentType, RouteMethod routeMethod ) {
            throw new NotImplementedException ();
        }

    }

}
