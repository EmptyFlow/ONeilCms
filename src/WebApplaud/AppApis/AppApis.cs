using System.Text.Json;
using WebApplaud.Models;

namespace WebApplaud.AppApis
{
	internal static class AppApis
	{

		internal static List<AppModel> m_apps = [];

		internal static string m_appFolder = "";

		internal static void Initialize(WebApplication app)
		{
			m_appFolder = Path.Combine(GlobalConfig.Path, "apps");
			var appDirectory = new DirectoryInfo(m_appFolder);
			var applicationFolders = appDirectory.GetDirectories();

			foreach (var application in applicationFolders)
			{
				var appName = application.Name;

				var manifestPath = Path.Combine(m_appFolder, appName, "webapp.json");
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
					Icon = manifestModel.Icon,
				};

				var basePath = $"{appmodel.LocalId}/";

				foreach (var file in application.EnumerateFiles("*", SearchOption.AllDirectories))
				{
					var directory = file.DirectoryName ?? "";
					var localFolder = directory.Replace(application.FullName, "").Replace("\\", "/");
					if (localFolder.Length > 0)
					{
						if (localFolder.First() == '/') localFolder = localFolder.Substring(1);
						if (localFolder.Last() != '/') localFolder = localFolder + '/';
					}
					//if ($"{localFolder}{file.Name}" == "appapi.js") continue;

					var mimeType = FolderInitiator.GetMimeTypeForFileExtension(file.Extension);
					var downloadName = mimeType != "text/html" ? file.Name : null;
					var lastModified = file.LastWriteTimeUtc;
					var fullName = file.FullName;
					app.MapGet($"{basePath}{localFolder}{file.Name}", () =>
					{
						var stream = File.OpenRead(fullName);
						return Results.File(stream, contentType: mimeType, fileDownloadName: downloadName, enableRangeProcessing: true, lastModified: lastModified);
					});
					/*app.MapGet($"{basePath}appapi.js", () =>
					{
						return Results.File([], contentType: "application/json", fileDownloadName: "appapi.js");
					});*/
				}
			}
		}

		internal static void RegisterRoutes(WebApplication app, Dictionary<string, string> appIdentifiers, bool maskRoutes = false)
		{
			app.MapGet($"apps", () =>
			{
				var apps = appIdentifiers
					.Select(a => new AppManifestModel { Name = a.Value, Id = a.Key })
					.ToArray();
				return Results.Json(apps, AppJsonSerializerContext.Default);
			});
			app.MapPost($"openapp", () =>
			{
				return Results.Content("/lala.html", contentType: "text/plain");
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

	}
}
