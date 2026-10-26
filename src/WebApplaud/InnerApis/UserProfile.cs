using WebApplaud.ApplicationApis;
using WebApplaud.Models;

namespace WebApplaud.InnerApis
{

	public static class UserProfile
	{

		internal static readonly Dictionary<string, UserSessionModel> m_sessions = [];

		internal static readonly List<UserProfileModel> m_profiles = [];

		internal static Dictionary<string, string> m_routes = [];

		internal static void Initialize()
		{
			//TODO: read real user profiles

			m_profiles.Add(
				new UserProfileModel
				{
					Id = "lalalalalal",
					Login = "test",
					Password = HashPassword.Hash("lala"),
				}
			);
		}

		internal static string AddRoute(string route, bool maskRoutes = false)
		{
			var registerRoute = maskRoutes ? Guid.NewGuid().ToString().Replace("-", "") : route;
			m_routes.Add(registerRoute, route);
			return registerRoute;
		}

		internal static void RegisterRoutes(WebApplication app, bool maskRoutes = false)
		{
			app.MapPost(
				AddRoute("login", maskRoutes), (HttpContext httpContext) =>
				{
					var form = httpContext.Request.Form;
					var login = form.ContainsKey("login") && form["login"].Any() ? form["login"].First() : "";
					var password = form.ContainsKey("password") && form["password"].Any() ? form["password"].First() : "";
					if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password)) return Results.BadRequest();

					password = HashPassword.Hash(password); // hash password

					var profile = m_profiles.FirstOrDefault(a => a.Login == login && a.Password == password);
					if (profile is null) return Results.BadRequest();

					var token = Guid.NewGuid().ToString();

					CreateProfileCookie(httpContext, token);

					var drawerApp = AppApis.GetDrawerApp();

					var session = new UserSessionModel
					{
						App = drawerApp.LocalId,
						Expire = DateTime.UtcNow.AddDays(1),
						Id = profile.Id,
						Token = token
					};

					return Results.Redirect($"/{drawerApp.LocalId}/{drawerApp.StartupPage}");
				}
			)
			.RequireRateLimiting("login");
		}

		internal static void CreateProfileCookie(HttpContext httpContext, string token)
		{
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
