using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using System.Runtime.CompilerServices;

namespace OnielCms.Core
{

	public record ProcessorElementParameter(string Name, string Value);

	public record ProcessorElement(string Name, IEnumerable<ProcessorElementParameter> Parameters);

	public static class HttpRouteHandler
	{

		private static Dictionary<string, RouteCache> m_routeHandler = [];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static RouteParameters GetRouteParameters(HttpRoute route, string path)
		{
			var queryParameters = QueryHelpers.ParseQuery(new Uri("http://localhost" + path).Query)
				.Select(a => new { a.Key, Items = a.Value.Select(b => b?.ToString() ?? "").ToArray() })
				.ToDictionary(a => a.Key, a => a.Items);

			return new RouteParameters(queryParameters, new Dictionary<string, string>());
		}

		public static void RegisterMethodHandlers(WebApplication app)
		{
			app.MapGet("/", async (
				HttpContext context,
				IMemoryCache cache,
				[FromServices] IRouteResponse routeResponseService,
				[FromServices] IProcessorsDeserializer processorsDeserializer) =>
			{
				return await Handler(context, "/", routeResponseService, processorsDeserializer, "GET", cache);
			}
);
			app.MapGet("/{*path}", async (
				HttpContext context,
				IMemoryCache cache,
				[FromRoute] string path,
				[FromServices] IRouteResponse routeResponseService,
				[FromServices] IProcessorsDeserializer processorsDeserializer) =>
			{
				return await Handler(context, path, routeResponseService, processorsDeserializer, "GET", cache);
			}
			);

			// Post Handler

			app.MapPost("/", async (
				HttpContext context,
				IMemoryCache cache,
				[FromServices] IRouteResponse routeResponseService,
				[FromServices] IProcessorsDeserializer processorsDeserializer) =>
			{
				return await Handler(context, "/", routeResponseService, processorsDeserializer, "POST", cache);
			}
			);
			app.MapPost("/{*path}", async (
				HttpContext context,
				IMemoryCache cache,
				[FromRoute] string path,
				[FromServices] IRouteResponse routeResponseService,
				[FromServices] IProcessorsDeserializer processorsDeserializer) =>
			{
				return await Handler(context, path, routeResponseService, processorsDeserializer, "POST", cache);
			}
			);

			// Put Handler

			app.MapPut("/", async (
				HttpContext context,
				IMemoryCache cache,
				[FromServices] IRouteResponse routeResponseService,
				[FromServices] IProcessorsDeserializer processorsDeserializer) =>
			{
				return await Handler(context, "/", routeResponseService, processorsDeserializer, "PUT", cache);
			}
			);
			app.MapPut("/{*path}", async (
				HttpContext context,
				IMemoryCache cache,
				[FromRoute] string path,
				[FromServices] IRouteResponse routeResponseService,
				[FromServices] IProcessorsDeserializer processorsDeserializer) =>
			{
				return await Handler(context, path, routeResponseService, processorsDeserializer, "PUT", cache);
			}
			);

			// Delete Handler

			app.MapDelete("/", async (
				HttpContext context,
				IMemoryCache cache,
				[FromServices] IRouteResponse routeResponseService,
				[FromServices] IProcessorsDeserializer processorsDeserializer) =>
			{
				return await Handler(context, "/", routeResponseService, processorsDeserializer, "DELETE", cache);
			}
			);
			app.MapDelete("/{*path}", async (
				HttpContext context,
				IMemoryCache cache,
				[FromRoute] string path,
				[FromServices] IRouteResponse routeResponseService,
				[FromServices] IProcessorsDeserializer processorsDeserializer) =>
			{
				return await Handler(context, path, routeResponseService, processorsDeserializer, "DELETE", cache);
			}
			);
		}

		public static async Task<IResult> Handler(
			HttpContext httpContext,
			string path,
			IRouteResponse routeResponse,
			IProcessorsDeserializer processorsDeserializer,
			string method,
			IMemoryCache cache)
		{
			try
			{
				var routeHandler = m_routeHandler[method];
				var routePair = routeHandler.GetRoute(path);
				if (routePair != null)
				{
					var route = routePair.Value.routeEntity;

					var parameters = GetRouteParameters(route, path);
					var response = await routeResponse.Get(route, cache, httpContext, routeHandler.Version ?? "", parameters, httpContext.RequestAborted);

					var processors = route.GetProcessors(processorsDeserializer);
					if (processors.Any())
					{
						var state = CreateState(httpContext, cache);

						foreach (var processor in processors)
						{
							if (m_routeProcessors.TryGetValue(processor.Name, out var processorAction))
							{
								processorAction?.Invoke(ref state, processor);
								if (!state.Handled && state.Result is not null) return state.Result;
								if (!state.Handled) return Results.StatusCode(500);
							}
						}

						if (state.Result != null) return state.Result;
					}

					if (response.Length == 0) return Results.StatusCode(204); // no content

					return Results.File(response, route.ContentType, fileDownloadName: !string.IsNullOrEmpty(route.DownloadFileName) ? route.DownloadFileName : null);
				}
				if (routePair == null) return Results.NotFound();

				return Results.Ok();
			}
			catch (StatusCodeException statusCodeException)
			{
				return Results.StatusCode(statusCodeException.StatusCode);
#if DEBUG
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
#else
            } catch {
#endif
				return Results.StatusCode(500);
			}
		}

		private static void FillHandler(string version, IEnumerable<HttpRoute> routes, Dictionary<string, RouteCache> handlerDictionary)
		{
			if (!routes.Any()) return;

			var handler = new RouteCache();
			handler.FillRoutesCache(version, routes);
			var method = routes.First().Method;

			handlerDictionary.Add(method, handler);
		}

		public static async Task LoadRoutesExtent(string version, IEnumerable<HttpRoute> routes)
		{
			m_routeHandler.Clear();

			Dictionary<string, RouteCache> handlers = new();
			foreach (var group in routes.GroupBy(a => a.Method))
			{
				FillHandler(version, group, handlers);
			}

			m_routeHandler = handlers;
		}

		private static readonly Dictionary<string, RouteProcessorDelegate> m_routeProcessors = [];

		public delegate ValueTask RouteProcessorDelegate(ref ProcessorState processorState, ProcessorElement processorElement);

		private static ProcessorState CreateState(HttpContext context, IMemoryCache memoryCache)
		{
			var flags = new Dictionary<string, bool>();
			return new ProcessorState
			{
				HttpContext = context,
				MemoryCache = memoryCache,
				Result = null,
				Handled = true,
				Flags = flags
			};
		}

		/// <summary>
		/// Add processor for text content.
		/// </summary>
		/// <param name="processor">Processor name.</param>
		/// <param name="callback">Callback.</param>
		public static void AddRouteProcessor(string processor, RouteProcessorDelegate callback)
		{
			if (m_routeProcessors.ContainsKey(processor)) throw new Exception($"Text processor with name {processor} already added!");

			m_routeProcessors.Add(processor, callback);
		}

		public struct ProcessorState
		{

			public HttpContext HttpContext;

			public IMemoryCache MemoryCache;

			public IResult? Result;

			public bool Handled;

			public Dictionary<string, bool> Flags;

			public readonly bool IsAuthenticated() => Flags["Authenticated"];

		}

	}

}
