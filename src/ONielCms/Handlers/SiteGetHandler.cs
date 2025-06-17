using Microsoft.AspNetCore.Mvc;
using ONielCms.Services.DatabaseLogic;
using ONielCommon.Storage.EntityServices;
using System.Text;
using System.Text.RegularExpressions;
using Route = ONielCommon.Entities.Route;

namespace ONielCms.Handlers {

    public static class SiteGetHandler {

        public static async Task<IResult> GetHandler (
            [FromRoute] string path,
            [FromServices] IRouteResponseService routeResponseService,
            [FromServices] IRouteService routeService ) {
            try {
                if ( !m_loaded ) await LoadRoutes ( routeService );

                var routePair = GetRoute ( path );
                if ( routePair != null ) {
                    var route = routePair.Value.routeId;
                    var response = await routeResponseService.GetResponse ( path, route.Id, m_currentVersion );
                    var content = Encoding.UTF8.GetString ( response.Item1 );
                    return Results.Content ( content, route.ContentType, Encoding.UTF8 );
                    //return Results.File ( response.Item1, route.ContentType, fileDownloadName: "test.html" );
                }
                if ( routePair == null ) return Results.NotFound ();

                return Results.Ok ();
            } catch {
                return Results.Problem ( statusCode: 500 );
            }
        }

        private static Dictionary<string, Route> m_getPreciousRoutes = new ();

        private static Dictionary<Regex, (string, Route)> m_getDynamicRoutes = new ();

        private static string m_currentVersion = "";

        private static bool m_loaded = false;

        private static (string route, Route routeId)? GetRoute ( string path ) {
            if ( m_getPreciousRoutes.TryGetValue ( path, out var routeId ) ) return (path, routeId);

            var dynamicRouteKey = m_getDynamicRoutes.Keys.FirstOrDefault ( a => a.IsMatch ( path ) );
            if ( dynamicRouteKey != null ) {
                var dynamicRoute = m_getDynamicRoutes[dynamicRouteKey];
                return (dynamicRoute.Item1, dynamicRoute.Item2);
            }

            return null;
        }

        public static async Task LoadRoutes ( IRouteService routeService ) {
            var (routes, currentVersion) = await routeService.GetRoutes ();

            m_currentVersion = currentVersion;

            m_getPreciousRoutes = routes
                .Where (
                    a =>
                        !a.Path.Contains ( "{" ) && !a.Path.Contains ( "}" ) // dynamic segment of path look like {XXX}
                )
                .ToDictionary ( a => a.Path, a => a );
            m_getDynamicRoutes = routes
                .Where (
                    a =>
                        a.Path.Contains ( "{" ) && a.Path.Contains ( "}" ) // dynamic segment of path look like {XXX}
                )
                .ToDictionary ( a => GetRegExForDynamicPath ( a.Path ), a => (a.Path, a) );

            m_loaded = true;
        }

        private static Regex GetRegExForDynamicPath ( string path ) {
            var result = path;
            foreach ( Match match in Regex.Matches ( path, @"\{[A-Za-z0-1]{0,}\}" ) ) {
                var segment = match.Value;

                result = result.Replace ( segment, @"\{[A-Za-z0-1]{0,}\}" );
            }

            return new Regex ( result.Replace ( "/", @"\/" ) );
        }

    }

}
