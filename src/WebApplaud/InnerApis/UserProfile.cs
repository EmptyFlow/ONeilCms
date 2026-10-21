using Microsoft.AspNetCore.Mvc;
using WebApplaud.ApplicationApis;
using WebApplaud.Models;

namespace WebApplaud.InnerApis
{

	public static class UserProfile
	{

		internal static readonly Dictionary<string, UserSessionModel> m_sessions = [];

		internal static Dictionary<string, string> m_routes = [];

		internal static void Initialize()
		{
			// read user profiles
		}

		internal static string AddRoute(string route, bool maskRoutes = false)
		{
			var registerRoute = maskRoutes ? Guid.NewGuid().ToString().Replace("-", "") : route;
			m_routes.Add(registerRoute, route);
			return registerRoute;
		}

		internal static void RegisterRoutes(WebApplication app, bool maskRoutes = false)
		{
			app.MapPost(AddRoute("login", maskRoutes), (HttpContext httpContext) =>
			{
				var token = Guid.NewGuid().ToString();

				var form = httpContext.Request.Form;

				CreateProfileCookie(httpContext, token);

				var drawerApp = AppApis.GetDrawerApp();

				var session = new UserSessionModel
				{
					App = drawerApp,
					Expire = DateTime.UtcNow.AddDays(1),
					Id = "111",
					Token = token
				};
				

				return Results.Redirect("");
			});
		}

		internal static void CreateProfileCookie(HttpContext httpContext, string token) {
			httpContext.Response.Cookies.Append("waut", token);
			var cookieOptions = new CookieOptions()
			{
				IsEssential = true,
				Path = "/",
				//Domain = "localhost",
				Expires = DateTime.Now.AddDays(100),
				HttpOnly = true,
				SameSite = SameSiteMode.Strict
			};

			httpContext.Response.Cookies.Append("waut", token, cookieOptions);
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
