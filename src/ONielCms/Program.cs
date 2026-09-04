using OnielCms.Core;
using ONielCms;
using ONielCms.Services;
using ONielCms.Services.DatabaseLogic;
using ONielCommon.Storage;

ConfigurationService.Initialize();
var storageService = new StorageContext(new ConsoleStorageLogger(), new ConfigurationService());

if (await CommandLineHandler.HandleCommandLine(storageService)) return;

Console.WriteLine("No commands passed, start to server");

var builder = WebApplication.CreateBuilder(Enumerable.Empty<string>().ToArray());

builder.Services.AddMemoryCache();
builder.Services.AddOutputCache(a =>
{
	// max body 2 Mb
	a.MaximumBodySize = 2 * 1024 * 1024;
	// size of storage 50 Mb
	a.SizeLimit = 50 * 1024 * 1024;
	a.DefaultExpirationTimeSpan = TimeSpan.FromMinutes(2);
});
builder.Services.AddResponseCaching();

Dependencies.Resolve(builder.Services);

var app = builder.Build();

//app.Urls.Add("http://localhost:4000");

app.UseOutputCache();
app.UseResponseCaching();
app.UseRouting();

using (var scope = app.Services.CreateScope())
{
	var configurationService = scope.ServiceProvider.GetService<IConfigurationService>();
	var routeService = scope.ServiceProvider.GetService<IRouteService>();

	if (configurationService != null && routeService != null)
	{
		var (routes, version) = await routeService.GetAllRoutesInCurrentVersion();

		HttpRouteHandler.LoadRoutesExtent(
			app,
			version,
			routes
				.Select(
					a => new HttpRoute
					{
						ContentType = a.ContentType,
						DownloadFileName = a.DownloadFileName,
						Id = a.Id,
						Method = a.Method,
						Path = a.Path,
						Processors = []
					}
				)
		);
	}
}

app.Run();