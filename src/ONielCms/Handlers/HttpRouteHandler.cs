using Microsoft.Extensions.Caching.Memory;
using ONielCms.Services.DatabaseLogic;
using ONielCommon.Exceptions;
using System.Collections.Concurrent;

namespace ONielCms.Handlers
{

	public class HttpRoute
	{

		public Guid Id { get; init; }

		public string Path { get; init; } = "";

		public string ContentType { get; init; } = "";

		public string Method { get; init; } = "";

		public string DownloadFileName { get; set; } = "";

		public string Processors { get; set; } = "";

	}

	public record ProcessorElementParameter(string Name, string Value);

	public record ProcessorElement(string Name, IEnumerable<ProcessorElementParameter> Parameters);

	public interface IProcessorsDeserializer
	{

		IEnumerable<ProcessorElement> Deserialize(string processors);

	}

	public static class HttpRouteExtensions
	{

		private static ConcurrentDictionary<string, IEnumerable<ProcessorElement>> _processorsCache = [];

		extension(HttpRoute target)
		{

			public IEnumerable<ProcessorElement> GetProcessors(IProcessorsDeserializer processorsDeserializer)
			{
				var processors = target.Processors;
				if (string.IsNullOrEmpty(processors)) return [];

				if (!_processorsCache.ContainsKey(processors)) return _processorsCache[processors];

				var processorItems = processorsDeserializer.Deserialize(processors);
				if (processorItems?.Any() == true) _processorsCache.TryAdd(processors, processorItems);

				return processorItems != null ? processorItems : [];
			}

		}

	}

	public static class HttpRouteHandler
	{

		private static Dictionary<string, RouteCache> m_routeHandler = [];

		public static async Task<IResult> Handler(
			HttpContext httpContext,
			string path,
			IRouteResponseService routeResponseService,
			IRouteService routeService,
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
					var response = await routeResponseService.GetResponse(path, route.Id, routeHandler.Version ?? "", httpContext.RequestAborted);

					var processors = route.GetProcessors(processorsDeserializer);
					if (processors.Any())
					{
						var state = CreateState(httpContext, cache);

						foreach (var processor in processors)
						{
							if (m_routeProcessors.TryGetValue(processor.Name, out var processorAction))
							{
								processorAction?.Invoke(ref state);
								if (!state.Handled && state.Result is not null) return state.Result;
								if (!state.Handled) return Results.StatusCode(500);
							}
						}

						if (state.Result != null) return state.Result;
					}

					return Results.File(response.Item1, route.ContentType, fileDownloadName: !string.IsNullOrEmpty(route.DownloadFileName) ? route.DownloadFileName : null);
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

		public delegate ValueTask RouteProcessorDelegate(ref ProcessorState processorState);

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
