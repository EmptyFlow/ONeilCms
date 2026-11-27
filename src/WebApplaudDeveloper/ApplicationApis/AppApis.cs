using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace WebApplaudDeveloper.ApplicationApis {

	internal static class AppApis {

		internal static Dictionary<string, string> m_routes = [];

		internal static AppApisInternal? m_internal;

		internal static async Task Initialize(WebApplication app) {
			m_internal = new AppApisInternal();
			await m_internal.Initialize(GlobalConfig.Path, app);
		}

		internal static void RegisterRoutes(WebApplication app, bool maskRoutes = false) {
			if (m_internal is null) return;

			app.MapGet("apps", (HttpContext httpContext) => {
				var apps = m_internal.Apps
					.Where(a => a.Id != m_internal.DefaultAppId)
					.Select(a => new AppDrawerModel { Title = a.Titles.FirstOrDefault().Value ?? "", Identifier = a.Id, Icon = a.Icon })
					.ToList();
				return Results.Json(apps, AppJsonSerializerContext.Default);
			});

			app.MapPost("openApp", (HttpContext httpContext, [FromBody] string app) => {
				var needOpenApp = m_internal.Apps.FirstOrDefault(a => a.Id == app);
				if (needOpenApp is null) return Results.NotFound();

				return Results.Content($"/{needOpenApp.LocalId}/{needOpenApp.StartupPage}", contentType: "text/plain");
			});
			app.MapPost("opendrawer", (HttpContext httpContext) => {
				var defaultApp = m_internal.Apps.FirstOrDefault(a => a.Id == m_internal.DefaultAppId);
				if (defaultApp is null) return Results.BadRequest();

				return Results.Content($"/{defaultApp.LocalId}/{defaultApp.StartupPage}", contentType: "text/plain");
			});
			app.MapPost("closeapp", (HttpContext httpContext) => {
				//TODO: remove app resources

				var defaultApp = m_internal.Apps.FirstOrDefault(a => a.Id == m_internal.DefaultAppId);
				if (defaultApp is null) return Results.BadRequest();

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

		internal string m_appFolder = "";

		internal List<AppModel> m_apps = [];

		internal readonly Dictionary<string ,string> m_appsSettings = new Dictionary<string, string>();

		internal string m_defaultAppId = "";

		internal AppApisInternal() {
		}

		public string AppFolder => m_appFolder;

		public IEnumerable<AppModel> Apps => m_apps;

		public string DefaultAppId => m_defaultAppId;

		internal async Task Initialize(string globalPath, IEndpointRouteBuilder app) {
			if (globalPath is null) throw new ArgumentNullException(nameof(globalPath));
			if (app is null) throw new ArgumentNullException(nameof(app));

			m_appFolder = globalPath;
			var applicationFolders = new DirectoryInfo(m_appFolder).GetDirectories();

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
						var stream = File.OpenRead(fullName);
						return Results.File(stream, contentType: mimeType, fileDownloadName: downloadName, enableRangeProcessing: true, lastModified: lastModified);
					});
				}
			}
		}

	}

}
