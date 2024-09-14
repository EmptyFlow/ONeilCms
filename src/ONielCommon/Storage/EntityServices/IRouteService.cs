using ONielCommon.Entities;
using SqlKata;

namespace ONielCommon.Storage.EntityServices {

    public interface IRouteService {

        Task<IEnumerable<Route>> GetRoutes ( Action<Query>? adjust = default );

        Task AddOrUpdate ( Route edition );

        Task Delete ( Guid id );

    }

}
