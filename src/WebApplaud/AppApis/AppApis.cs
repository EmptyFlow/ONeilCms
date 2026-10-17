using WebApplaud.Models;

namespace WebApplaud.AppApis
{
	internal static class AppApis
	{

		public static void RegisterRoutes(WebApplication app, Dictionary<string, string> appIdentifiers, bool maskRoutes = false)
		{
			app.MapGet($"apps", () =>
			{
				var apps = appIdentifiers
					.Select(a => new AppModel { Name = a.Value, Id = a.Key })
					.ToArray();
				return Results.Json(apps, AppJsonSerializerContext.Default);
			});
			app.MapPost($"openapp", () =>
			{
				return Results.Content("/lala.html", contentType: "text/plain");
			});
		}

		internal static void RegisterRoutes(WebApplication app)
		{
			throw new NotImplementedException();
		}
	}
}
