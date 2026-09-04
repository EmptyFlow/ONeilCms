using Microsoft.AspNetCore.Http;

namespace OnielCms.Core
{
	public interface IRouteResponse
	{

		Task<byte[]> GetFile(Guid id, string version, HttpContext httpContext, CancellationToken cancellationToken = default);

	}

}
