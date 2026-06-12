using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace OnielCms.Core
{
	public interface IRouteResponse
	{

		Task<byte[]> Get(HttpRoute httpRoute, IMemoryCache cache, HttpContext httpContext, string version, RouteParameters routeParameters, CancellationToken cancellationToken = default);

	}

}
