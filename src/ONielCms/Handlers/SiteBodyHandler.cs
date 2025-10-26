﻿using ONielCms.Services.DatabaseLogic;
using ONielCommon.Entities;
using ONielCommon.Storage;
using System.Text;

namespace ONielCms.Handlers {

    public static class SiteBodyHandler {

        private static Dictionary<string, RouteHandler> m_routeHandler = new Dictionary<string, RouteHandler> ();

        public static async Task<IResult> Handler (
            HttpContext httpContext,
            string path,
            IRouteResponseService routeResponseService,
            IRouteService routeService,
            string method ) {
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

                        if ( !string.IsNullOrEmpty ( route.Processors ) ) content = await RunProcesors ( route.Processors, content );

                        return Results.Content ( content, route.ContentType, Encoding.UTF8 );
                    }

                }
                if ( routePair == null ) return Results.NotFound ();

                return Results.Ok ();
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

        private static Dictionary<string, Func<string, Task<string>>> m_textContent = new ();

        private static async Task<string> RunProcesors ( string processors, string content ) {
            var processorsList = processors
                .Split ( "," )
                .Select ( a => a.Trim () )
                .ToList ();

            var result = content;

            foreach ( var processor in processorsList ) {
                if ( m_textContent.TryGetValue ( processor, out var callback ) ) {
                    result = await callback ( content );
                }
            }

            return result;
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

        /// <summary>
        /// Add processor for text content.
        /// </summary>
        /// <param name="processor">Processor name.</param>
        /// <param name="callback">CAllback.</param>
        public static void AddTextProcessor ( string processor, Func<string, Task<string>> callback ) {
            if ( m_textContent.ContainsKey ( processor ) ) throw new Exception ( $"Processor with name {processor} already added!" );

            m_textContent.Add ( processor, callback );
        }

    }

}
