namespace ONielCms.Services.DatabaseLogic {

    public interface IRouteResponseService {

        public Task<(byte[], int)> GetResponse ( string path );

    }

}
