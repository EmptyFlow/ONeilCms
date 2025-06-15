namespace ONielCms.Services.DatabaseLogic {

    public interface IRouteResponseService {

        public Task<(byte[], int)> GetResponse ( string path );

        public Task<(byte[], int)> GetResponse ( string path, Guid routeId, string version );

    }

}
