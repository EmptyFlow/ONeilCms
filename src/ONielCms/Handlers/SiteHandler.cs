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

        public static async Task LoadRoutes (IRouteService routeService ) {
            var routes = await routeService.GetRoutes (a => new Query().Where("method", "GET"));

            var refreshRoutes = new Dictionary<string, Guid> ();

            refreshRoutes = routes.ToDictionary ( a => a.Path, a => a.Id );

            m_getPreciousRoutes = refreshRoutes;
        }

    }

}
