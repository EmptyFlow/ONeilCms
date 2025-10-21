using ONielCms.Services.DatabaseLogic;
using ONielCommon.Storage.EntityServices;
using System.Text;

namespace ONielCms.Handlers {

    public static class SiteBodyHandler {

        public static async Task<IResult> Handler (
            HttpContext httpContext,
            string path,
            IRouteResponseService routeResponseService,
            IRouteService routeService,
            string method) {
            try {
                if ( !m_loaded ) await LoadRoutes ( routeService, method );

                var routePair = m_routeHandler?.GetRoute ( path );
                if ( routePair != null ) {
                    var route = routePair.Value.routeId;
                    var response = await routeResponseService.GetResponse ( path, route.Id, m_routeHandler?.Version ?? "", httpContext.RequestAborted );

                    if ( route.DownloadAsFile && !string.IsNullOrEmpty ( route.DownloadFileName ) ) {
                        return Results.File ( response.Item1, route.ContentType, fileDownloadName: route.DownloadFileName );
                    } else {
                        var content = Encoding.UTF8.GetString ( response.Item1 );
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

        private static RouteHandler? m_routeHandler = null;

        private static bool m_loaded = false;

        public static async Task LoadRoutes ( IRouteService routeService, string method ) {
            var (routes, currentVersion) = await routeService.GetAllRoutesInCurrentVersion ( method );

            m_routeHandler = new RouteHandler ();
            m_routeHandler.FillRoutesCache ( currentVersion, routes );

            m_loaded = true;
        }

    }

}
