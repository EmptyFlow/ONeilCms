using Microsoft.Extensions.Caching.Memory;

namespace ONielCommon.Processors {

    public static class CookieAuthorizationProcessor {

        public static Task<string> Process ( string content, HttpContext context, IMemoryCache memoryCache ) {
            return Task.FromResult ( "" );
        }

    }

}
