using Microsoft.Extensions.Caching.Memory;
using OnielCms.Core;
using ONielCms.Services.DatabaseLogic;

namespace ONielCms.Services
{
	public class RouteResponse : IRouteResponse
	{

		private readonly IRouteResponseService m_routeResponseService;

		public RouteResponse(IRouteResponseService routeResponseService) => m_routeResponseService = routeResponseService;

		public async Task<byte[]> Get(HttpRoute httpRoute, IMemoryCache cache, HttpContext httpContext, string version, RouteParameters routeParameters, CancellationToken cancellationToken = default)
		{
			return await m_routeResponseService.GetResponse(httpRoute.Id, version, httpContext.RequestAborted);
		}

	}

}
