using Microsoft.AspNetCore.Mvc;
    using ONielCommon.Storage.EntityServices;

namespace ONielCms.Handlers {

    public static class SiteHandler {

        public static IResult GetHandler (
            [FromRoute] string path,
            [FromServices] IRouteService routeService ) {

            if ( m_preciousRoutes.TryGetValue ( path, out var routeId ) ) {

            } else {
                return Results.NotFound ();
            }

            return Results.Ok ();
        }

        private static Dictionary<string, Guid> m_preciousRoutes = new ();

        public static async Task LoadRoutes (IRouteService routeService ) {
            var routes = await routeService.GetRoutes ();

            var refreshRoutes = new Dictionary<string, Guid> ();

            refreshRoutes = routes.ToDictionary ( a => a.Path, a => a.Id );

            m_preciousRoutes = refreshRoutes;
        }

    }

}
