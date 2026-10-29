using System.Collections.Concurrent;
using System.Text.Json;
using WebApplaud.ApplicationApis;
using WebApplaud.Databases.KeyValue;
using WebApplaud.Models;

namespace WebApplaud.InnerApis
{

	public static class UserProfile
	{

		internal static readonly ConcurrentDictionary<string, UserSessionModel> m_sessions = [];

		internal static readonly List<UserProfileModel> m_profiles = [];

		internal static Dictionary<string, string> m_routes = [];

		internal static KeyValueDatabase m_profilesDatabase = new KeyValueDatabase();

		internal static KeyValueDatabase m_sessionDatabase = new KeyValueDatabase();

		async internal static Task Initialize(bool onlyProfiles = false)
		{
			var profilesFile = Path.Combine(GlobalConfig.Path, "settings", "profiles.kvdb");
			await m_profilesDatabase.LoadDatabase(profilesFile);

			var keys = m_profilesDatabase.GetKeys();

			foreach (var key in keys)
			{
				var model = m_profilesDatabase.GetString(key);
				UserProfileModel? userProfile = JsonSerializer.Deserialize(model, AppJsonSerializerContext.Default.UserProfileModel);
				if (userProfile is null) continue;

				m_profiles.Add(userProfile);
			}

			if (onlyProfiles) return;

			var sessionsFile = Path.Combine(GlobalConfig.Path, "settings", "sessions.kvdb");
			await m_sessionDatabase.LoadDatabase(sessionsFile);

			var tokens = m_sessionDatabase.GetKeys();

			var changed = false;
			foreach (var token in tokens)
			{
				var model = m_sessionDatabase.GetString(token);
				UserSessionModel? userSession = JsonSerializer.Deserialize(model, AppJsonSerializerContext.Default.UserSessionModel);
				if (userSession is null) continue;
				if (userSession.Expire < DateTime.UtcNow)
				{
					m_sessionDatabase.Remove(token);
					changed = true;
					continue;
				}

				if (userSession.Token != token) continue; // if item was created with incorrect key we ignore it

				m_sessions.TryAdd(token, userSession);
			}
			if (changed) await m_sessionDatabase.SaveDatabase();
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
				AddRoute("login", maskRoutes), async (HttpContext httpContext) =>
				{
					var form = httpContext.Request.Form;
					var login = form.ContainsKey("login") && form["login"].Any() ? form["login"].First() : "";
					var password = form.ContainsKey("password") && form["password"].Any() ? form["password"].First() : "";
					if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password)) return Results.BadRequest();

					password = HashPassword.Hash(password); // hash password

					var profile = m_profiles.FirstOrDefault(a => a.Login == login && a.Password == password);
					if (profile is null) return Results.BadRequest();

					var token = Guid.NewGuid().ToString();
					var drawerApp = AppApis.GetDrawerApp();
					var session = new UserSessionModel
					{
						App = drawerApp.LocalId,
						Expire = DateTime.UtcNow.AddDays(7),
						Id = profile.Id,
						Token = token
					};
					if (!m_sessions.TryAdd(token, session)) return Results.BadRequest();

					try
					{
						m_sessionDatabase.SetString(token, JsonSerializer.Serialize(session, AppJsonSerializerContext.Default.UserSessionModel));
						await m_sessionDatabase.SaveDatabase();
					}
					catch
					{
						//if we can't save session to database we remove token from memory
						m_sessions.Remove(token, out _);
					}

					CreateProfileCookie(httpContext, token);

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
				Expires = DateTime.UtcNow.AddDays(7),
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

		internal static async Task CreateUserProfile(UserProfileModel userProfile)
		{
			userProfile.Id = Guid.NewGuid().ToString().Replace("-", "");
			if (m_profiles.Any(a => a.Id == userProfile.Id)) throw new Exception($"User with id {userProfile.Id} already created!");
			if (m_profiles.Any(a => a.Login == userProfile.Login)) throw new Exception($"User with login {userProfile.Login} already created!");

			if (userProfile.FirstName.Length > 60) userProfile = userProfile with { FirstName = userProfile.FirstName.Substring(0, 60) };
			if (userProfile.LastName.Length > 100) userProfile = userProfile with { FirstName = userProfile.FirstName.Substring(0, 100) };

			m_profilesDatabase.SetString(userProfile.Id, JsonSerializer.Serialize(userProfile, AppJsonSerializerContext.Default.UserProfileModel));
			await m_profilesDatabase.SaveDatabase();
		}

	}

}
