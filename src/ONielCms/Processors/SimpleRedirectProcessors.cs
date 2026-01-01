using Microsoft.Extensions.Caching.Memory;

namespace ONielCms.Processors {

    public static class SimpleRedirectProcessors {

        public static ValueTask<bool> Process ( HttpContext httpContext, IMemoryCache memoryCache ) {
            //var result = true;
            var token = httpContext.Request.Cookies.Where ( a => a.Key == "capst" ).Select ( a => a.Value ).FirstOrDefault ();
            if ( string.IsNullOrEmpty ( token ) || !memoryCache.TryGetValue ( token, out _ ) ) {
                httpContext.Response.StatusCode = 401;
                return new ValueTask<bool> ( false );
            }

            return new ValueTask<bool> ( true );
        }

    }

}
