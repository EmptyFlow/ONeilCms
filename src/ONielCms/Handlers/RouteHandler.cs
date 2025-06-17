using System.Text.RegularExpressions;
using Route = ONielCommon.Entities.Route;

namespace ONielCms.Handlers {
    public class RouteHandler {

        private Dictionary<string, Route> m_preciousRoutes = new ();

        private Dictionary<Regex, (string, Route)> m_dynamicRoutes = new ();

        private string m_version = "";

        public string Version => m_version;

        public void FillRoutesCache ( string version, IEnumerable<Route> routes ) {
            m_version = version;
            if ( routes == null ) return;

            m_preciousRoutes = routes
                .Where (
                    a =>
                        !a.Path.Contains ( "{" ) && !a.Path.Contains ( "}" ) // dynamic segment of path look like {XXX}
                )
                .ToDictionary ( a => a.Path, a => a );
            m_dynamicRoutes = routes
                .Where (
                    a =>
                        a.Path.Contains ( "{" ) && a.Path.Contains ( "}" ) // dynamic segment of path look like {XXX}
                )
                .ToDictionary ( a => GetRegExForDynamicPath ( a.Path ), a => (a.Path, a) );
        }

        private Regex GetRegExForDynamicPath ( string path ) {
            var result = path;
            foreach ( Match match in Regex.Matches ( path, @"\{[A-Za-z0-1]{0,}\}" ) ) {
                var segment = match.Value;

                result = result.Replace ( segment, @"\{[A-Za-z0-1]{0,}\}" );
            }

            return new Regex ( result.Replace ( "/", @"\/" ) );
        }

        public (string route, Route routeId)? GetRoute ( string path ) {
            if ( m_preciousRoutes.TryGetValue ( path, out var routeId ) ) return (path, routeId);

            var dynamicRouteKey = m_dynamicRoutes.Keys.FirstOrDefault ( a => a.IsMatch ( path ) );
            if ( dynamicRouteKey != null ) {
                var dynamicRoute = m_dynamicRoutes[dynamicRouteKey];
                return (dynamicRoute.Item1, dynamicRoute.Item2);
            }

            return null;
        }

    }

}
