using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebApplaud.Databases.KeyValue;
using WebApplaud.FileSystems;
using WebApplaud.InnerApis;
using WebApplaud.Models;

namespace WebApplaud.ApplicationApis {
	internal static class AppApis {

		internal static Dictionary<string, string> m_routes = [];

		internal static KeyValueDatabase m_appsSettings = new KeyValueDatabase();

		internal static AppApisInternal? m_internal;

		internal static async Task Initialize(IInternalFileSystem internalFileSystem, WebApplication app) {
			m_internal = new AppApisInternal(internalFileSystem, m_appsSettings);
			await m_internal.Initialize(GlobalConfig.Path, app);
		}

		internal static string AddRoute(string route, bool maskRoutes = false) {
			var registerRoute = maskRoutes ? Guid.NewGuid().ToString().Replace("-", "") : route;
			m_routes.Add(registerRoute, route);
			return registerRoute;
		}

		internal static void RegisterRoutes(WebApplication app, bool maskRoutes = false) {
			if (m_internal is null) return;

			app.MapGet(AddRoute("apps", maskRoutes), (HttpContext httpContext) => {
				var session = UserProfile.GetProfileFromSession(httpContext);
				if (session is null) return Results.Unauthorized();

				var apps = m_internal.Apps
					.Where(a => a.Id != m_internal.DefaultAppId)
					.Select(a => new AppDrawerModel { Title = a.Titles.FirstOrDefault().Value ?? "", Identifier = a.Id, Icon = a.Icon })
					.ToList();
				return Results.Json(apps, AppJsonSerializerContext.Default);
			});

			app.MapPost(AddRoute("openApp", maskRoutes), (HttpContext httpContext, [FromBody] string app) => {
				var session = UserProfile.GetProfileFromSession(httpContext);
				if (session is null) return Results.Unauthorized();

				var needOpenApp = m_internal.Apps.FirstOrDefault(a => a.Id == app);
				if (needOpenApp is null) return Results.NotFound();

				session.App = needOpenApp.LocalId;

				return Results.Content($"/{needOpenApp.LocalId}/{needOpenApp.StartupPage}", contentType: "text/plain");
			});
			app.MapPost(AddRoute("opendrawer", maskRoutes), (HttpContext httpContext) => {
				var session = UserProfile.GetProfileFromSession(httpContext);
				if (session is null) return Results.Unauthorized();

				var defaultApp = m_internal.Apps.FirstOrDefault(a => a.Id == m_internal.DefaultAppId);
				if (defaultApp is null) return Results.BadRequest();

				session.App = m_internal.DefaultAppId;

				return Results.Content($"/{defaultApp.LocalId}/{defaultApp.StartupPage}", contentType: "text/plain");
			});
			app.MapPost(AddRoute("closeapp", maskRoutes), (HttpContext httpContext) => {
				var session = UserProfile.GetProfileFromSession(httpContext);
				if (session is null) return Results.Unauthorized();

				//TODO: remove app resources

				var defaultApp = m_internal.Apps.FirstOrDefault(a => a.Id == m_internal.DefaultAppId);
				if (defaultApp is null) return Results.BadRequest();

				session.App = m_internal.DefaultAppId;

				return Results.Content($"/{defaultApp.LocalId}/{defaultApp.StartupPage}", contentType: "text/plain");
			});
		}

		internal static string GetAppLocalId(string appIdentifier) {
			if (m_internal is null) throw new Exception("m_internal is null");

			var app = m_internal.Apps.FirstOrDefault(a => a.Id == appIdentifier);
			if (app is null) return "";

			return app.LocalId;
		}

		internal static string GetAppId(string appLocalId) {
			if (m_internal is null) throw new Exception("m_internal is null");

			var app = m_internal.Apps.FirstOrDefault(a => a.LocalId == appLocalId);
			if (app is null) return "";

			return app.Id;
		}

		internal static AppModel? GetDrawerApp() => m_internal?.Apps.FirstOrDefault(a => a.Id == m_internal.DefaultAppId);

	}


	internal sealed class AppApisInternal {

		internal readonly IInternalFileSystem m_internalFileSystem;

		internal string m_appFolder = "";

		internal List<AppModel> m_apps = [];

		internal readonly IKeyValueDatabase m_appsSettings;

		internal string m_defaultAppId = "";

		internal AppApisInternal(IInternalFileSystem internalFileSystem, IKeyValueDatabase keyValueDatabase) {
			m_internalFileSystem = internalFileSystem ?? throw new ArgumentNullException(nameof(internalFileSystem));
			m_appsSettings = keyValueDatabase ?? throw new ArgumentNullException(nameof(keyValueDatabase));
		}

		public string AppFolder => m_appFolder;

		public IEnumerable<AppModel> Apps => m_apps;

		public string DefaultAppId => m_defaultAppId;

		internal async Task Initialize(string globalPath, IEndpointRouteBuilder app) {
			m_appFolder = Path.Combine(globalPath, "apps");
			var applicationFolders = m_internalFileSystem.GetDirectories(m_appFolder);

			foreach (var application in applicationFolders) {
				var appName = application.Name;

				var manifestPath = Path.Combine(m_appFolder, appName, "webapp.json");
				if (!File.Exists(manifestPath)) continue;

				var manifestJson = File.ReadAllText(manifestPath);
				var manifestModel = JsonSerializer.Deserialize(manifestJson, AppJsonSerializerContext.Default.AppManifestModel);
				if (manifestModel is null) continue;

				var appmodel = new AppModel {
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

				foreach (var file in application.EnumerateFiles()) {
					var directory = file.DirectoryName ?? "";
					var appLocalId = appmodel.LocalId;
					var localFolder = directory.Replace(application.FullName, "").Replace("\\", "/");
					if (localFolder.Length > 0) {
						if (localFolder.First() == '/') localFolder = localFolder.Substring(1);
						if (localFolder.Last() != '/') localFolder = localFolder + '/';
					}
					var mimeType = FolderInitiator.GetMimeTypeForFileExtension(file.Extension);
					var isHtml = mimeType.ToLowerInvariant() == "text/html";
					var downloadName = !isHtml ? file.Name : null;
					var lastModified = file.LastWriteTimeUtc;
					var fullName = file.FullName;
					app.MapGet($"{basePath}{localFolder}{file.Name}", (HttpContext httpContext) => {
						var session = UserProfile.GetProfileFromSession(httpContext);
						if (session is null) return Results.Unauthorized();
						if (session.App != appLocalId) return Results.Unauthorized(); // check if open appropriate application

						var stream = File.OpenRead(fullName);
						return Results.File(stream, contentType: mimeType, fileDownloadName: downloadName, enableRangeProcessing: true, lastModified: lastModified);
					});
				}
			}

			await LoadDatabase(globalPath);
		}

		internal async Task LoadDatabase(string globalPath) {
			var settingsFolder = Path.Combine(globalPath, "settings", "apps.kvdb");
			await m_appsSettings.LoadDatabase(settingsFolder);
			var defautlApp = m_appsSettings.GetString("defaultapp");
			if (string.IsNullOrEmpty(defautlApp) || !m_apps.Any(a => a.Id == defautlApp)) {
				m_defaultAppId = m_apps.First().Id;
			}
			else {
				m_defaultAppId = defautlApp;
			}
		}

	}

}
