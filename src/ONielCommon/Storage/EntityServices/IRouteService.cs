using ONielCommon.Entities;

namespace ONielCommon.Storage.EntityServices {

    public interface IRouteService {

        Task<(IEnumerable<Route>, string)> GetRoutes ();

        Task<(IEnumerable<Route>, string)> PostRoutes ();

        Task AddOrUpdate ( Route edition );

        Task Delete ( Guid id );

    }

}
