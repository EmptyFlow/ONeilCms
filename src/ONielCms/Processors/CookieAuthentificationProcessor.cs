using ONielCms.Extensions;
using System.Runtime.CompilerServices;
using static ONielCms.Handlers.SiteBodyHandler;

namespace ONielCms.Processors
{

    /// <summary>
    /// Cookie authentification checker.
    /// </summary>
    public static class CookieAuthentificationProcessor
    {

        private const string DefaultCookieKey = "capst";

        public static ValueTask CheckAndExitAuthentification(ref ProcessorState state, ProcessorElement processorElement)
        {
            var cookieKey = GetCookieKey(processorElement);
            var token = state.HttpContext.Request.Cookies.Where(a => a.Key == cookieKey).Select(a => a.Value).FirstOrDefault();
            if (string.IsNullOrEmpty(token) || !state.MemoryCache.TryGetValue(token, out _))
            {
                state.Handled = false;
                state.Result = Results.StatusCode(401);
                return ProcessorsShared.EmptyValueTask;
            }

            state.Handled = true;
            return ProcessorsShared.EmptyValueTask;
        }

        public static ValueTask CheckAuthentification(ref ProcessorState state, ProcessorElement processorElement)
        {
            var cookieKey = GetCookieKey(processorElement);
            var token = state.HttpContext.Request.Cookies.Where(a => a.Key == cookieKey).Select(a => a.Value).FirstOrDefault();
            state.Handled = true;
            if (string.IsNullOrEmpty(token) || !state.MemoryCache.TryGetValue(token, out _))
            {
                state.Flags["Authenticated"] = true;
                return ProcessorsShared.EmptyValueTask;
            }

            return ProcessorsShared.EmptyValueTask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetCookieKey(ProcessorElement processorElement)
        {
            return processorElement.Parameters.FirstOrDefault(a => a.Name == "CookieKey")?.Value ?? DefaultCookieKey;
        }

    }

}
