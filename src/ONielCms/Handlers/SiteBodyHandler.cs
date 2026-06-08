using Microsoft.Extensions.Caching.Memory;
using ONielCms.Services.DatabaseLogic;
using ONielCommon.Entities;
using ONielCommon.Exceptions;
using ONielCommon.Storage;
using ONielCms.Extensions;

namespace ONielCms.Handlers
{

    public static class SiteBodyHandler
    {

        private static Dictionary<string, RouteHandler> m_routeHandler = [];

        public static async Task<IResult> Handler(
            HttpContext httpContext,
            string path,
            IRouteResponseService routeResponseService,
            IRouteService routeService,
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

                    var processors = route.GetProcessors();
                    if (processors.Any())
                    {
                        var state = CreateState(httpContext, cache);

                        foreach (var processor in processors)
                        {
                            if (m_textProcessors.TryGetValue(processor.Name, out var processorAction))
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

        private static void FillHandler(string version, IEnumerable<SiteRoute> routes, Dictionary<string, RouteHandler> handlerDictionary)
        {
            if (!routes.Any()) return;

            var handler = new RouteHandler();
            handler.FillRoutesCache(version, routes);
            var method = routes.First().Method;

            handlerDictionary.Add(method, handler);
        }

        public static async Task LoadAllRoutesInCurrentVersion(IConfigurationService configurationService)
        {
            Dictionary<string, RouteHandler> handlers = new();
            var storageContext = new StorageContext(new ConsoleStorageLogger(), configurationService);
            var routeService = new RouteService(storageContext);

            var (routes, version) = await routeService.GetAllRoutesInCurrentVersion();

            foreach (var group in routes.GroupBy(a => a.Method))
            {
                FillHandler(version, group, handlers);
            }

            m_routeHandler = handlers;
        }

        private static readonly Dictionary<string, RouteProcessorDelegate> m_textProcessors = [];

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
            if (m_textProcessors.ContainsKey(processor)) throw new Exception($"Text processor with name {processor} already added!");

            m_textProcessors.Add(processor, callback);
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
