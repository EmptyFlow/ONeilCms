using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

		private static Dictionary<string, RouteCache> m_routeHandler = [];

		public static async Task<IResult> FileHandler(HttpContext httpContext, IRouteResponse routeResponse, Guid id, string version, string contentType, string? downloableName = default)
		{
			try
			{
				var response = await routeResponse.GetFile(id, version, httpContext);
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


		private static void FillHandler(WebApplication app, string version, IEnumerable<HttpRoute> routes, Dictionary<string, RouteCache> handlerDictionary)
		{
			if (!routes.Any()) return;

			var handler = new RouteCache();
			handler.FillRoutesCache(version, routes);
			var method = routes.First().Method;

			handlerDictionary.Add(method, handler);

			foreach (var route in routes)
			{
				if (route.Method.ToLowerInvariant() == "get" && !route.Processors.Any())
				{
					app.MapGet(route.Path, async (
						HttpContext context,
						[FromServices] IRouteResponse routeResponseService) =>
						{
							return await FileHandler(context, routeResponseService, route.Id, version, route.ContentType, route.DownloadFileName);
						}
					);
				}
				if (route.Method.ToLowerInvariant() == "get" && route.Processors.Any())
				{
					app.MapGet(route.Path, async (
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
		}

		public static async Task LoadRoutesExtent(WebApplication app, string version, IEnumerable<HttpRoute> routes)
		{
			m_routeHandler.Clear();

			Dictionary<string, RouteCache> handlers = new();
			FillHandler(app, version, routes, handlers);

			m_routeHandler = handlers;
		}

	}

}
