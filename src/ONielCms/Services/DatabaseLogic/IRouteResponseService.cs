namespace ONielCms.Services.DatabaseLogic {

    public interface IRouteResponseService {

        public Task<byte[]> GetResponse ( Guid routeId, string version, CancellationToken cancellationToken = default );

    }

}
