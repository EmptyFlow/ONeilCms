namespace ONielCms.Services.DatabaseLogic {

    public interface IResourceManagementService {

        Task CreateResource ( string identifier, string edition, byte[] content );

        Task CreateResourceVersion ( string identifier, string edition, byte[] content );

        Task EditResourceVersion ( string identifier, string edition, byte[] content );

    }

}
