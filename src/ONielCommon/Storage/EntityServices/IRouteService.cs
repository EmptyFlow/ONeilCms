using ONielCommon.Entities;

namespace ONielCommon.Storage.EntityServices {

    public interface IRouteService {

        Task<(IEnumerable<SiteRoute>, string)> GetRoutes ();

        Task<(IEnumerable<SiteRoute>, string)> PostRoutes ();

        Task AddOrUpdate ( SiteRoute edition );

        Task Delete ( Guid id );

    }

}
