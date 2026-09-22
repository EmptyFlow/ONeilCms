using Microsoft.AspNetCore.StaticFiles;
using System.Reflection;
using WebApplaud;

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

var builder = WebApplication.CreateBuilder(args);

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
foreach (var application in applications)
{
	var appName = application.Name;
	var id = Guid.NewGuid().ToString().Replace("-", "");
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
		if ($"{localFolder}{file.Name}" == "appapi.js") continue;

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
	}

	app.MapGet($"{basePath}appapi.js", () =>
	{
		return Results.File([], contentType: "application/json", fileDownloadName: "appapi.js");
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

//app.Urls.Add("http://localhost:4000");

app.Run();
