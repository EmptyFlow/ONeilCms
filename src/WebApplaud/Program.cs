using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.Reflection;
using WebApplaud;
using WebApplaud.Models;

Version? version = Assembly.GetEntryAssembly()?.GetName().Version;
string? fileVersion = version is not null ? $"{version.Major}.{version.Minor}.{version.Build}" : "";
Console.WriteLine("WebApplaud version " + fileVersion);
Console.WriteLine("(c) 2026 Roman Vladimirov. All rights reserved.\n");

GlobalConfig.SetupPath("C:/work/Repositories/AppsFolder");
if (FolderInitiator.FoldersNotExists(GlobalConfig.Path))
{
	try
	{
		FolderInitiator.CreateFolderStructure(GlobalConfig.Path);
	}
	catch
	{
		return;
	}
}

ThreadPool.SetMinThreads(workerThreads: 200, completionPortThreads: 200);

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
	serverOptions.Limits.MaxConcurrentConnections = 10000;
	serverOptions.Limits.MaxConcurrentUpgradedConnections = 5000;
});
//builder.WebHost.UseSockets();
builder.Services.ConfigureHttpJsonOptions(options =>
{
	options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddMemoryCache();
builder.Services.AddOutputCache(a =>
{
	// max body 2 Mb
	a.MaximumBodySize = 2 * 1024 * 1024;
	// size of storage 50 Mb
	a.SizeLimit = 50 * 1024 * 1024;
	a.DefaultExpirationTimeSpan = TimeSpan.FromMinutes(5);
});
builder.Services.AddResponseCaching();

//Dependencies.Resolve(builder.Services);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment()) app.MapOpenApi();

if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();

app.UseOutputCache();
app.UseResponseCaching();
app.UseRouting();

var appsDirectory = new DirectoryInfo(Path.Combine(GlobalConfig.Path, "apps"));
var applications = appsDirectory.GetDirectories();
var appIdentifiers = new Dictionary<string, string>();
var fileIdentifiers = new Dictionary<string, string>();
var folderIdentifiers = new Dictionary<string, string>();
var folderRoots = new Dictionary<string, FolderRootModel>();

IndexingUserFolders(fileIdentifiers, folderIdentifiers, folderRoots);

InitializeApplications(app, applications, appIdentifiers);

InitializeAppApi(app, fileIdentifiers, folderIdentifiers, folderRoots);

//app.Urls.Add("http://localhost:4000");

app.Run();

static void InitializeApplications(WebApplication app, DirectoryInfo[] applications, Dictionary<string, string> appIdentifiers)
{
	foreach (var application in applications)
	{
		var appName = application.Name;
#if DEBUG
		var id = "testdebugapp";
#else
	var id = Guid.NewGuid().ToString().Replace("-", "");
#endif
		appIdentifiers.Add(id, appName);

		var basePath = $"{id}/";

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

			var mimeType = GetMimeTypeForFileExtension(file.Extension);
#if DEBUG
			Console.WriteLine($"{basePath}{localFolder}{file.Name}");
#endif
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

static void IndexingUserFolders(Dictionary<string, string> fileIdentifiers, Dictionary<string, string> folderIdentifiers, Dictionary<string, FolderRootModel> folderRoots)
{
	var commonDirectory = new DirectoryInfo(Path.Combine(GlobalConfig.Path, "data/common"));
	var root = new FolderRootModel { Path = "/data/common" };
	folderRoots.Add(Guid.NewGuid().ToString(), root);
	var lastFolderRoot = folderRoots.Last();
	Console.WriteLine("Root: " + lastFolderRoot.Key + " " + lastFolderRoot.Value);

	foreach (var directory in commonDirectory.EnumerateDirectories("*", SearchOption.AllDirectories))
	{
		root.Folders.Add(Guid.NewGuid().ToString(), directory.FullName.Replace('\\', '/').Replace(GlobalConfig.Path, ""));
		var last = root.Folders.Last();
		Console.WriteLine("Folder: " + last.Key + " " + last.Value);
	}
	foreach (var file in commonDirectory.EnumerateFiles("*", SearchOption.AllDirectories))
	{
		root.Files.Add(Guid.NewGuid().ToString(), file.FullName.Replace('\\', '/').Replace(GlobalConfig.Path, ""));
	}
}

static void InitializeAppApi(WebApplication app, Dictionary<string, string> fileIdentifiers, Dictionary<string, string> folderIdentifiers, Dictionary<string, FolderRootModel> folderRoots)
{
	app.MapGet($"folderroots", () =>
	{
		if (folderRoots.Count != 0)
		{
			var roots = string.Join(',', folderRoots.Keys.Select(a => $"\"{a}\""));
			return Results.Content($"[{roots}]", contentType: "application/json");
		}
		else
		{
			return Results.Content("[]", contentType: "application/json");
		}
	});
	app.MapGet($"folderitems", ([FromQuery] string rootId, [FromQuery] string? id = default) =>
	{
		if (folderRoots.TryGetValue(rootId, out var rootItem))
		{
			var rootPath = rootItem.Path + "/";
			//get root common folder
			if (string.IsNullOrEmpty(id))
			{
				var files = rootItem.Files
					.Where(a => a.Value.Replace(rootPath, "").IndexOf("/") == -1)
					.Select(a => new FolderItemModel { Name = a.Value.Replace(rootPath, ""), Id = a.Key })
					.ToArray();
				var folders = rootItem.Folders
					.Where(a => a.Value.Replace(rootPath, "").IndexOf("/") == -1)
					.Select(a => new FolderItemModel { Name = a.Value.Replace(rootPath, ""), Id = a.Key })
					.ToArray();
				return Results.Json(new FolderItemsModel { Files = files, Folders = folders }, AppJsonSerializerContext.Default);
			}
			else
			{
				if (rootItem.Folders.TryGetValue(id, out var folderRoot))
				{
					var rootFolderPath = folderRoot + "/";
					var files = rootItem.Files
						.Where(a => a.Value.Replace(rootFolderPath, "").IndexOf("/") == -1)
						.Select(a => new FolderItemModel { Name = a.Value.Replace(rootFolderPath, ""), Id = a.Key })
						.ToArray();
					var folders = rootItem.Folders
						.Where(a => a.Value.Replace(rootFolderPath, "").IndexOf("/") == -1)
						.Select(a => new FolderItemModel { Name = a.Value.Replace(rootFolderPath, ""), Id = a.Key })
						.ToArray();
					return Results.Json(new FolderItemsModel { Files = files, Folders = folders }, AppJsonSerializerContext.Default);
				}
			}
		}

		return Results.NotFound();
	});
	app.MapGet($"file", ([FromQuery] string rootId, [FromQuery] string id) =>
	{
		if (folderRoots.TryGetValue(rootId, out var rootItem))
		{
			if (rootItem.Files.TryGetValue(id, out var fileItem))
			{
				var mimeType = GetMimeTypeForFileExtension(fileItem);
				var fullPath = Path.Combine(GlobalConfig.Path, fileItem.StartsWith('/') ? fileItem.Substring(1) : fileItem);
				var fileName = Path.GetFileName(fullPath);
				if (!File.Exists(fullPath)) return Results.NotFound();

				var stream = File.OpenRead(fullPath);
				return Results.File(stream, contentType: mimeType, enableRangeProcessing: true, fileDownloadName: fileName);
			}
		}

		return Results.NotFound();
	});
	app.MapPost($"settextcontext", async ([FromQuery] string rootId, [FromQuery] string id, [FromBody] string content, CancellationToken cancellationToken) => {
		if (folderRoots.TryGetValue(rootId, out var rootItem))
		{
			if (rootItem.Files.TryGetValue(id, out var fileItem))
			{
				var fullPath = Path.Combine(GlobalConfig.Path, fileItem.StartsWith('/') ? fileItem.Substring(1) : fileItem);
				if (!File.Exists(fullPath)) return Results.Content("false", contentType: "application/json");

				await File.WriteAllTextAsync(fullPath, content, cancellationToken);
				return Results.Content("true", contentType: "application/json");
			}
		}

		return Results.Content("false", contentType: "application/json");
	});
}

static string GetMimeTypeForFileExtension(string filePath)
{
	const string DefaultContentType = "application/octet-stream";

	var provider = new FileExtensionContentTypeProvider();

	if (!provider.TryGetContentType(filePath, out var contentType))
	{
		contentType = DefaultContentType;
	}

	return contentType;
}
