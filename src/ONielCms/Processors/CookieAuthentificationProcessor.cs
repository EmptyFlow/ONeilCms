using static ONielCms.Handlers.SiteBodyHandler;

namespace ONielCms.Processors {

    /// <summary>
    /// Simple checker
    /// </summary>
    public static class CookieAuthentificationProcessor {

        public static ValueTask CheckAndExitAuthentification ( ref ProcessorState state ) {
            var token = state.HttpContext.Request.Cookies.Where ( a => a.Key == "capst" ).Select ( a => a.Value ).FirstOrDefault ();
            if ( string.IsNullOrEmpty ( token ) || !state.MemoryCache.TryGetValue ( token, out _ ) ) {
                state.HttpContext.Response.StatusCode = 401; // return 401 code
                state.Handled = false;
                return ProcessorsShared.EmptyValueTask;
            }

            state.Handled = true;
            return ProcessorsShared.EmptyValueTask;
        }

        public static ValueTask CheckAuthentification ( ref ProcessorState state ) {
            var token = state.HttpContext.Request.Cookies.Where ( a => a.Key == "capst" ).Select ( a => a.Value ).FirstOrDefault ();
            state.Handled = true;
            if ( string.IsNullOrEmpty ( token ) || !state.MemoryCache.TryGetValue ( token, out _ ) ) {
                state.Flags["Authenticated"] = true;
                return ProcessorsShared.EmptyValueTask;
            }

            return ProcessorsShared.EmptyValueTask;
        }

    }

}
