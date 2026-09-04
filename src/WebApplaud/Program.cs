
using Microsoft.Extensions.FileProviders;
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
/*builder.Services.AddOutputCache(a =>
{
	// max body 2 Mb
	a.MaximumBodySize = 2 * 1024 * 1024;
	// size of storage 50 Mb
	a.SizeLimit = 50 * 1024 * 1024;
	a.DefaultExpirationTimeSpan = TimeSpan.FromMinutes(5);
});*/
builder.Services.AddResponseCaching();

//Dependencies.Resolve(builder.Services);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment()) app.MapOpenApi();

if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();

app.UseStaticFiles(new StaticFileOptions
{
	FileProvider = new PhysicalFileProvider(Path.Combine(GlobalConfig.Path, "apps")),
	RequestPath = "/apps"
});

//app.UseOutputCache();
app.UseResponseCaching();
app.UseRouting();

//app.Urls.Add("http://localhost:4000");

app.Run();
