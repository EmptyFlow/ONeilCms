using ONielCommon.Entities;

namespace ONielCommon.Storage.EntityServices {

    public interface IRouteService {

        Task<IEnumerable<SiteRoute>> GetAllRoutesInVersion ( string version, string method );

        Task<(IEnumerable<SiteRoute>, string)> GetAllRoutesInCurrentVersion ( string method );

        Task AddOrUpdate ( SiteRoute edition );

        Task Delete ( Guid id );

    }

}
