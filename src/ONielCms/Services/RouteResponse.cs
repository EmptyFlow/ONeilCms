using Microsoft.Extensions.Caching.Memory;
using OnielCms.Core;
using ONielCms.Services.DatabaseLogic;

namespace ONielCms.Services
{
	public class RouteResponse : IRouteResponse
	{

		private readonly IRouteResponseService m_routeResponseService;

		public RouteResponse(IRouteResponseService routeResponseService) => m_routeResponseService = routeResponseService;

		public Task<byte[]> GetFile(Guid id, string version, HttpContext httpContext, CancellationToken cancellationToken = default)
		{
			return m_routeResponseService.GetResponse(id, version, httpContext.RequestAborted);
		}

	}

}
