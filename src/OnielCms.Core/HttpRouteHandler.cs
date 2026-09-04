using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace OnielCms.Core
{

	public record ProcessorElementParameter(string Name, string Value);

	public delegate ValueTask<IResult> ProcessorHandler(HttpContext httpContext, IEnumerable<ProcessorElementParameter> parameters);

	public record ProcessorElement(string Name, ProcessorHandler ProcessorHandler, IEnumerable<ProcessorElementParameter> Parameters);

	public class NullResult : IResult
	{
		public Task ExecuteAsync(HttpContext httpContext) => throw new NotImplementedException();
	}

	public static class HttpRouteHandler
	{

		public static async Task<IResult> FileHandler(HttpContext httpContext, IRouteResponse routeResponse, Guid id, string version, string contentType, string? downloableName = default, CancellationToken cancellationToken = default)
		{
			try
			{
				var response = await routeResponse.GetFile(id, version, httpContext, cancellationToken);
				if (response.Length == 0) return Results.StatusCode(204); // no content

				return Results.File(response, contentType, fileDownloadName: !string.IsNullOrEmpty(downloableName) ? downloableName : null);
			}
#if DEBUG
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
#else
            } catch {
#endif
				return Results.StatusCode(500);
			}
		}

		public static void LoadRoutesExtent(WebApplication app, string version, IEnumerable<HttpRoute> routes)
		{
			if (!routes.Any()) return;

			foreach (var route in routes)
			{
				RouteHandlerBuilder? builder = null;

				if (route.Method.ToLowerInvariant() == "get")
				{
					if (!route.Processors.Any())
					{
						builder = app.MapGet(route.Path, async (
							HttpContext context,
							[FromServices] IRouteResponse routeResponseService) =>
						{
							return await FileHandler(context, routeResponseService, route.Id, version, route.ContentType, route.DownloadFileName);
						}
						);
					}
					else
					{
						builder = app.MapGet(route.Path, async (
							HttpContext context,
							[FromServices] IRouteResponse routeResponseService) =>
						{
							for (var i = 0; i < route.Processors.Count(); i++)
							{
								var processor = route.Processors[i];
								var result = await processor.ProcessorHandler(context, processor.Parameters);
								if (result != null && result != ProcessorsShared.NoResult) return result;
							}

							return await FileHandler(context, routeResponseService, route.Id, version, route.ContentType, route.DownloadFileName);
						}
						);
					}
				}

				// configure server cache
				if (route.ServerCacheResponseSeconds.HasValue && route.ServerCacheResponseSeconds.Value > 0 && builder is not null)
				{
					builder = builder.CacheOutput(p => p.Expire(TimeSpan.FromSeconds(route.ServerCacheResponseSeconds.Value)));
				}

				// configure client cache
				if (route.ClientCacheResponseSeconds.HasValue && route.ClientCacheResponseSeconds.Value > 0 && builder is not null)
				{
					builder.WithMetadata(new ResponseCacheAttribute { Duration = route.ClientCacheResponseSeconds.Value });
				}
			}
		}

	}

}
