using ONielCommon.Entities;

namespace ONielCommon.Storage.EntityServices {

    public interface IRouteService {

        Task<IEnumerable<Route>> GetRoutes ();

        Task AddOrUpdate ( Route edition );

        Task Delete ( Guid id );

    }

}
