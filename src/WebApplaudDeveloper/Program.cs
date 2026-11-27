using System.Reflection;
using WebApplaudDeveloper;
using WebApplaudDeveloper.ApplicationApis;

Version? version = Assembly.GetEntryAssembly()?.GetName().Version;
string? fileVersion = version is not null ? $"{version.Major}.{version.Minor}.{version.Build}" : "";

// if required we handle commands from Command line interface
//if (await ConsoleCommandsHandler.Handle(fileVersion)) return;

GlobalConfig.SetupPath("C:/work/Repositories/AppsFolder");

// if not we run web server
Console.WriteLine("WebApplaudDeveloper version " + fileVersion);
Console.WriteLine("(c) 2026 Roman Vladimirov. All rights reserved.\n");


var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options => {
	options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();

var app = builder.Build();

if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();

app.UseResponseCaching();
app.UseRouting();

FolderApi.RegisterRoutes(app, []);
AppApis.RegisterRoutes(app);

app.Run();


