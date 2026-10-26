using Microsoft.AspNetCore.RateLimiting;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.RateLimiting;
using WebApplaud;
using WebApplaud.ApplicationApis;
using WebApplaud.InnerApis;
using WebApplaud.Models;
using WebApplaud.Pages;

[assembly: InternalsVisibleToAttribute("WebApplaudTests")]

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
builder.Services.AddRateLimiter(options =>
{
	options.AddFixedWindowLimiter("login", options =>
	{
		options.Window = TimeSpan.FromSeconds(20);
		options.PermitLimit = 4;
		options.QueueLimit = 2;
		options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
	});
});

builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment()) app.MapOpenApi();

if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();

app.UseResponseCaching();
app.UseRouting();
app.UseRateLimiter();

var appsDirectory = new DirectoryInfo(Path.Combine(GlobalConfig.Path, "apps"));
var applications = appsDirectory.GetDirectories();
var appIdentifiers = new Dictionary<string, string>();
var folderRoots = new Dictionary<string, FolderRootModel>();

IndexingUserFolders(folderRoots);

UserProfile.Initialize();
AppApis.Initialize(app);
LoginPage.Initialize();

UserProfile.RegisterRoutes(app);
FolderApi.RegisterRoutes(app, folderRoots);
AppApis.RegisterRoutes(app);
LoginPage.RegisterRoutes(app);

//app.Urls.Add("http://localhost:4000");

app.Run();

static void IndexingUserFolders(Dictionary<string, FolderRootModel> folderRoots)
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