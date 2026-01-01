using Microsoft.Extensions.Caching.Memory;
using ONielCms.Services.DatabaseLogic;
using ONielCommon.Entities;
using ONielCommon.Exceptions;
using ONielCommon.Storage;
using System.Buffers;
using System.Text;

namespace ONielCms.Handlers {

    public static class SiteBodyHandler {

        private static Dictionary<string, RouteHandler> m_routeHandler = [];

        public static async Task<IResult> Handler (
            HttpContext httpContext,
            string path,
            IRouteResponseService routeResponseService,
            IRouteService routeService,
            string method,
            IMemoryCache cache ) {
            try {
                var routeHandler = m_routeHandler[method];
                var routePair = routeHandler.GetRoute ( path );
                if ( routePair != null ) {
                    var route = routePair.Value.routeId;
                    var response = await routeResponseService.GetResponse ( path, route.Id, routeHandler.Version ?? "", httpContext.RequestAborted );

                    if ( route.DownloadAsFile && !string.IsNullOrEmpty ( route.DownloadFileName ) ) {
                        return Results.File ( response.Item1, route.ContentType, fileDownloadName: route.DownloadFileName );
                    } else {
                        var content = Encoding.UTF8.GetString ( response.Item1 );

                        if ( !string.IsNullOrEmpty ( route.Processors ) ) content = await RunTextProcessors ( route.Processors, content, httpContext, cache );

                        return Results.Content ( content, route.ContentType, Encoding.UTF8 );
                    }

                }
                if ( routePair == null ) return Results.NotFound ();

                return Results.Ok ();
            } catch ( StatusCodeException statusCodeException ) {
                return Results.StatusCode ( statusCodeException.StatusCode );
#if DEBUG
            } catch ( Exception ex ) {
                Console.WriteLine ( ex.ToString () );
#else
            } catch {
#endif
                return Results.StatusCode ( 500 );
            }
        }

        private static void FillHandler ( string version, IEnumerable<SiteRoute> routes, Dictionary<string, RouteHandler> handlerDictionary ) {
            if ( !routes.Any () ) return;

            var handler = new RouteHandler ();
            handler.FillRoutesCache ( version, routes );
            var method = routes.First ().Method;

            handlerDictionary.Add ( method, handler );
        }

        public static async Task LoadAllRoutesInCurrentVersion ( IConfigurationService configurationService ) {
            Dictionary<string, RouteHandler> handlers = new ();
            var storageContext = new StorageContext ( new ConsoleStorageLogger (), configurationService );
            var routeService = new RouteService ( storageContext );

            var (routes, version) = await routeService.GetAllRoutesInCurrentVersion ();

            foreach ( var group in routes.GroupBy ( a => a.Method ) ) {
                FillHandler ( version, group, handlers );
            }

            m_routeHandler = handlers;
        }

        private static readonly Dictionary<string, TextProcessorDelegate> m_textProcessors = [];

        public delegate ValueTask TextProcessorDelegate ( ref ProcessorState processorState );

        private static async Task<string> RunTextProcessors ( string processors, string content, HttpContext context, IMemoryCache memoryCache ) {
            var processorsList = processors
                .Split ( "," )
                .Select ( a => a.Trim () )
                .ToArray ();

            var flags = new Dictionary<string, bool> ();
            var state = new ProcessorState {
                HttpContext = context,
                MemoryCache = memoryCache,
                TextContent = content,
                Handled = true,
                Flags = flags
            };
            var result = content;

            var processorActions = m_textProcessors.Where ( a => processorsList.Contains ( a.Key ) );

            foreach ( var processorAction in processorActions ) {
                await processorAction.Value ( ref state );
                if ( !state.Handled ) break;
            }

            return state.TextContent;
        }

        /// <summary>
        /// Add processor for text content.
        /// </summary>
        /// <param name="processor">Processor name.</param>
        /// <param name="callback">Callback.</param>
        public static void AddTextProcessor ( string processor, TextProcessorDelegate callback ) {
            if ( m_textProcessors.ContainsKey ( processor ) ) throw new Exception ( $"Text processor with name {processor} already added!" );

            m_textProcessors.Add ( processor, callback );
        }

        public struct ProcessorState {

            public HttpContext HttpContext;

            public IMemoryCache MemoryCache;

            public string TextContent;

            public bool Handled;

            public Dictionary<string, bool> Flags;

            public readonly bool IsAuthenticated () => Flags["Authenticated"];

        }

    }

}
