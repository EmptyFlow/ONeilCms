using ONielCommon.Storage;
using ONielCommon.Storage.EntityServices;

namespace ONielCms.Services {

    public class RouteResponseService : IRouteResponseService {

        private readonly IStorageContext m_storageContext;

        public RouteResponseService(IStorageContext storageContext) => m_storageContext = storageContext ?? throw new ArgumentNullException(nameof(storageContext));

        public Task<byte[]> GetRouteResponse ( Guid id ) {
            throw new NotImplementedException();
        }

    }

}
