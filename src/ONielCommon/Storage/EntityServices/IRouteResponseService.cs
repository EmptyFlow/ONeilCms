namespace ONielCommon.Storage.EntityServices {

    public interface IRouteResponseService {

        /// <summary>
        /// Get route response.
        /// </summary>
        /// <param name="id">Identifier.</param>
        /// <returns>Content.</returns>
        Task<byte[]> GetRouteResponse ( Guid id );

    }

}
