using Microsoft.Extensions.Caching.Memory;
using OnielCms.Core;
using ONielCms.Services.DatabaseLogic;

namespace ONielCms.Services
{
	public class RouteResponse : IRouteResponse
	{

		private readonly IRouteResponseService m_routeResponseService;

		public RouteResponse(IRouteResponseService routeResponseService) => m_routeResponseService = routeResponseService;

		public async Task<byte[]> Get(HttpRoute httpRoute, IMemoryCache cache, HttpContext httpContext, string version, CancellationToken cancellationToken = default)
		{
			return await m_routeResponseService.GetResponse(httpRoute.Id, version, httpContext.RequestAborted);
		}

		public Task<byte[]> GetFile(Guid id, string version, HttpContext httpContext)
		{
			return m_routeResponseService.GetResponse(id, version, httpContext.RequestAborted);
		}

	}

}
