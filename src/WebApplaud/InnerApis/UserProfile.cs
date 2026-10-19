using WebApplaud.Models;

namespace WebApplaud.InnerApis
{

	public static class UserProfile
	{

		private static readonly Dictionary<string, UserSessionModel> m_sessions = [];

		internal static void Initialize(WebApplication app)
		{
			// read user profiles
		}

		internal static void RegisterRoutes(WebApplication app, bool maskRoutes = false)
		{
			
		}

		internal static UserSessionModel? GetProfileFromSession(HttpContext httpContext)
		{
			var cookieValue = httpContext.Request.Cookies.FirstOrDefault(a => a.Key == "waut").Value ?? null;
			if (cookieValue is null || string.IsNullOrEmpty(cookieValue)) return null;

			if (m_sessions.ContainsKey(cookieValue))
			{
				var session = m_sessions[cookieValue];
				if (DateTime.UtcNow >= session.Expire) return null;

				return session;
			}

			return null;
		}

	}

}
