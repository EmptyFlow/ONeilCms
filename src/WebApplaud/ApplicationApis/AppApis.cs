using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebApplaud.InnerApis;
using WebApplaud.Models;

namespace WebApplaud.ApplicationApis
{
	internal static class AppApis
	{

		internal static List<AppModel> m_apps = [];

		internal static string m_appFolder = "";

		internal static Dictionary<string, string> m_routes = [];

		internal static void Initialize(WebApplication app)
		{
			m_appFolder = Path.Combine(GlobalConfig.Path, "apps");
			var appDirectory = new DirectoryInfo(m_appFolder);
			var applicationFolders = appDirectory.GetDirectories();

			foreach (var application in applicationFolders)
			{
				var appName = application.Name;

				var manifestPath = Path.Combine(m_appFolder, appName, "webapp.json");
				if (!File.Exists(manifestPath)) continue;

				var manifestJson = File.ReadAllText(manifestPath);
				var manifestModel = JsonSerializer.Deserialize(manifestJson, AppJsonSerializerContext.Default.AppManifestModel);
				if (manifestModel is null) continue;

				var appmodel = new AppModel
				{
					Id = manifestModel.Id,
					LocalId = Guid.NewGuid().ToString().Replace("-", ""),
					Titles = manifestModel.Titles.ToDictionary(a => a.Key, a => a.Value),
					Developers = manifestModel.Developers.ToArray(),
					Factors = manifestModel.Factors.ToArray(),
					DownloadUrl = manifestModel.DownloadUrl,
					ManifestUrl = manifestModel.ManifestUrl,
					StartupPage = manifestModel.StartupPage,
					Icon = manifestModel.Icon,
				};
				m_apps.Add(appmodel);

				var basePath = $"{appmodel.LocalId}/";

				foreach (var file in application.EnumerateFiles("*", SearchOption.AllDirectories))
				{
					var directory = file.DirectoryName ?? "";
					var appLocalId = appmodel.LocalId;
					var localFolder = directory.Replace(application.FullName, "").Replace("\\", "/");
					if (localFolder.Length > 0)
					{
						if (localFolder.First() == '/') localFolder = localFolder.Substring(1);
						if (localFolder.Last() != '/') localFolder = localFolder + '/';
					}
					var mimeType = FolderInitiator.GetMimeTypeForFileExtension(file.Extension);
					var downloadName = mimeType != "text/html" ? file.Name : null;
					var lastModified = file.LastWriteTimeUtc;
					var fullName = file.FullName;
					app.MapGet($"{basePath}{localFolder}{file.Name}", (HttpContext httpContext) =>
					{
						var session = UserProfile.GetProfileFromSession(httpContext);
						if (session is null) return Results.Unauthorized();
						if (session.App != appLocalId) return Results.Unauthorized(); // check if open appropriate application

						var stream = File.OpenRead(fullName);
						return Results.File(stream, contentType: mimeType, fileDownloadName: downloadName, enableRangeProcessing: true, lastModified: lastModified);
					});
				}
			}
		}

		internal static string AddRoute(string route, bool maskRoutes = false)
		{
			var registerRoute = maskRoutes ? Guid.NewGuid().ToString().Replace("-", "") : route;
			m_routes.Add(registerRoute, route);
			return registerRoute;
		}

		internal static void RegisterRoutes(WebApplication app, bool maskRoutes = false)
		{
			app.MapGet(AddRoute("apps", maskRoutes), (HttpContext httpContext) =>
			{
				var session = UserProfile.GetProfileFromSession(httpContext);
				if (session is null) return Results.Unauthorized();

				var apps = m_apps
					.Select(a => new AppDrawerModel { Title = a.Titles.FirstOrDefault().Value ?? "", Identifier = a.Id })
					.ToList();
				return Results.Json(apps, AppJsonSerializerContext.Default);
			});

			app.MapPost(AddRoute("openApp", maskRoutes), (HttpContext httpContext, [FromQuery] string app) =>
			{
				var session = UserProfile.GetProfileFromSession(httpContext);
				if (session is null) return Results.Unauthorized();

				var application = m_apps.FirstOrDefault(a => a.Id == app);
				if (application is null) return Results.NotFound();

				var applicationModel = m_apps.FirstOrDefault(a => a.LocalId == session.App);
				if (applicationModel is null) return Results.NotFound();

				session.App = application.LocalId;

				return Results.Content($"/{applicationModel.LocalId}/{applicationModel.StartupPage}", contentType: "text/plain");
			});
		}

		internal static string GetAppLocalId(string appIdentifier)
		{
			var app = m_apps.FirstOrDefault(a => a.Id == appIdentifier);
			if (app is null) return "";

			return app.LocalId;
		}

		internal static string GetAppId(string appLocalId)
		{
			var app = m_apps.FirstOrDefault(a => a.LocalId == appLocalId);
			if (app is null) return "";

			return app.Id;
		}

		internal static string GetDrawerApp()
		{
			return m_apps.First().LocalId;
		}

	}
}
