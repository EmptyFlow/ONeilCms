using ONielCommon.Entities;

namespace ONielCms.Services.DatabaseLogic {

    public interface IRouteService {

        Task<IEnumerable<SiteRoute>> GetAllRoutesInVersion ( string version, string method );

        Task<(IEnumerable<SiteRoute>, string)> GetAllRoutesInCurrentVersion ( string method );

        Task<(IEnumerable<SiteRoute>, string)> GetAllRoutesInCurrentVersion ();

        Task AddOrUpdate ( SiteRoute edition );

        Task Delete ( Guid id );

    }

}
