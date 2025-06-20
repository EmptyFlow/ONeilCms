﻿using ONielCms.Services.DatabaseLogic;
using ONielCommon.Storage.EntityServices;
using System.Text;

namespace ONielCms.Handlers {

    public static class SitePostHandler {

        public static async Task<IResult> Handler ( HttpContext httpContext, string path, IRouteResponseService routeResponseService, IRouteService routeService ) {
            try {
                if ( !m_loaded ) await LoadRoutes ( routeService );

                var routePair = m_routeHandler?.GetRoute ( path ) ?? null;
                if ( routePair != null ) {
                    var route = routePair.Value.routeId;
                    var response = await routeResponseService.GetResponse ( path, route.Id, m_routeHandler?.Version ?? "" );

                    if ( route.DownloadAsFile && !string.IsNullOrEmpty ( route.DownloadFileName ) ) {
                        return Results.File ( response.Item1, route.ContentType, fileDownloadName: route.DownloadFileName );
                    } else {
                        var content = Encoding.UTF8.GetString ( response.Item1 );
                        return Results.Content ( content, route.ContentType, Encoding.UTF8 );
                    }
                }
                if ( routePair == null ) return Results.NotFound ();

                return Results.Ok ();
            } catch {
                return Results.Problem ( statusCode: 500 );
            }
        }

        private static RouteHandler? m_routeHandler = null;

        private static bool m_loaded = false;

        public static async Task LoadRoutes ( IRouteService routeService ) {
            var (routes, currentVersion) = await routeService.GetAllRoutesInCurrentVersion ( "POST" );

            m_routeHandler = new RouteHandler ();
            m_routeHandler.FillRoutesCache ( currentVersion, routes );

            m_loaded = true;
        }

    }

}
