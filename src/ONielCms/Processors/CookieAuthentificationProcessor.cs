using OnielCms.Core;
using System.Runtime.CompilerServices;

namespace ONielCms.Processors
{

	/// <summary>
	/// Cookie authentification checker.
	/// </summary>
	public static class CookieAuthentificationProcessor
	{

		private const string DefaultCookieKey = "capst";

		public static ValueTask<IResult> CheckAndExitAuthentification(HttpContext httpContext, IEnumerable<ProcessorElementParameter> parameters)
		{
			var cookieKey = GetCookieKey(parameters);
			var token = httpContext.Request.Cookies.Where(a => a.Key == cookieKey).Select(a => a.Value).FirstOrDefault();
			if (string.IsNullOrEmpty(token)) return ProcessorsShared.Status401Task;

			return ProcessorsShared.NoResultTask;
		}

		public static ValueTask<IResult> CheckAuthentification(HttpContext httpContext, IEnumerable<ProcessorElementParameter> parameters)
		{
			var cookieKey = GetCookieKey(parameters);
			var token = httpContext.Request.Cookies.Where(a => a.Key == cookieKey).Select(a => a.Value).FirstOrDefault();
			if (string.IsNullOrEmpty(token)) return ProcessorsShared.NoResultTask;

			return ProcessorsShared.NoResultTask;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static string GetCookieKey(IEnumerable<ProcessorElementParameter> parameters)
		{
			return parameters.FirstOrDefault(a => a.Name == "CookieKey")?.Value ?? DefaultCookieKey;
		}

	}

}
