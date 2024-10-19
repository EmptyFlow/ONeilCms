namespace ONielCms.Services.DatabaseLogic {

    public enum RouteMethod {
        GET = 1,
        POST = 2,
        PUT = 3,
        DELETE = 4,
        PATCH = 5
    };

    public interface IRouteManagementService {

        /// <summary>
        /// Create new route.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <param name="contentType">Content type or result.</param>
        /// <param name="routeMethod">What HTTP method will be handled.</param>
        /// <returns>Identifier created route.</returns>
        Task<Guid> Create ( string path, string contentType, RouteMethod routeMethod );

        /// <summary>
        /// Edit existing route.
        /// </summary>
        /// <param name="id">Identifier of existing route.</param>
        /// <param name="path">Path.</param>
        /// <param name="contentType">Content type or result.</param>
        /// <param name="routeMethod">What HTTP method will be handled.</param>
        Task Edit ( Guid id, string path, string contentType, RouteMethod routeMethod );

        /// <summary>
        /// Attach multiple resources.
        /// </summary>
        /// <param name="id">Route identifier.</param>
        /// <param name="resourceIds">Resource identifiers with order where it must be inserted.</param>
        Task AttachResources ( Guid id, IDictionary<Guid, int> resourceIds );

        /// <summary>
        /// Detach multiple resources.
        /// </summary>
        /// <param name="id">Route identifier.</param>
        /// <param name="resourceIds">Resource identifiers.</param>
        Task DetachResources ( Guid id, IEnumerable<int> resourceIds );

    }

}
