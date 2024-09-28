using Microsoft.AspNetCore.Mvc;
using ONielCommon.Storage.EntityServices;
using SqlKata;
using System.Text.RegularExpressions;

namespace ONielCms.Handlers {

    public static class SiteHandler {

        public static IResult GetHandler (
            [FromRoute] string path,
            [FromServices] IRouteService routeService ) {

            var route = GetRoute ( path );
            if ( route == null ) return Results.NotFound ();

            return Results.Ok ();
        }

        private static Dictionary<string, Guid> m_getPreciousRoutes = new ();

        private static Dictionary<Regex, (string, Guid)> m_getDynamicRoutes = new ();

        private static (string route, Guid routeId)? GetRoute ( string path ) {
            if ( m_getPreciousRoutes.TryGetValue ( path, out var routeId ) ) return (path, routeId);

            var dynamicRouteKey = m_getDynamicRoutes.Keys.FirstOrDefault ( a => a.IsMatch ( path ) );
            if ( dynamicRouteKey != null ) {
                var dynamicRoute = m_getDynamicRoutes[dynamicRouteKey];
                return (dynamicRoute.Item1, dynamicRoute.Item2);
            }

            return null;
        }

        public static async Task LoadRoutes ( IRouteService routeService ) {
            var routes = await routeService.GetRoutes ( a => new Query ().Where ( "method", "GET" ) );
            m_getPreciousRoutes = routes
                .Where (
                    a =>
                        !a.Path.Contains ( "{" ) && !a.Path.Contains ( "}" ) // dynamic segment of path look like {XXX}
                )
                .ToDictionary ( a => a.Path, a => a.Id );
            m_getDynamicRoutes = routes
                .Where (
                    a =>
                        a.Path.Contains ( "{" ) && a.Path.Contains ( "}" ) // dynamic segment of path look like {XXX}
                )
                .ToDictionary ( a => GetRegExForDynamicPath ( a.Path ), a => (a.Path, a.Id) );

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
