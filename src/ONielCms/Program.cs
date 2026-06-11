using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ONielCms;
using ONielCms.Handlers;
using ONielCms.Services;
using ONielCms.Services.DatabaseLogic;
using ONielCommon.Storage;

ConfigurationService.Initialize();
var storageService = new StorageContext(new ConsoleStorageLogger(), new ConfigurationService());

if (await CommandLineHandler.HandleCommandLine(storageService)) return;

Console.WriteLine("No commands passed, start to server");

var builder = WebApplication.CreateBuilder(Enumerable.Empty<string>().ToArray());

builder.Services.AddMemoryCache();

Dependencies.Resolve(builder.Services);

var app = builder.Build();

// load routes from current version and cache it
using (var scope = app.Services.CreateScope())
{
	var configurationService = scope.ServiceProvider.GetService<IConfigurationService>();
	var routeService = scope.ServiceProvider.GetService<IRouteService>();

	if (configurationService != null && routeService != null)
	{
		Dictionary<string, RouteCache> handlers = [];
		var (routes, version) = await routeService.GetAllRoutesInCurrentVersion();

		await HttpRouteHandler.LoadRoutesExtent(
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
						Processors = a.Processors
					}
				)
		);
	}
}

//app.Urls.Add("http://localhost:4000");

app.UseRouting();

// Get Handler

app.MapGet("/", async (
	HttpContext context,
	IMemoryCache cache,
	[FromServices] IRouteResponseService routeResponseService,
	[FromServices] IRouteService routeService,
	[FromServices] IProcessorsDeserializer processorsDeserializer) =>
{
	return await HttpRouteHandler.Handler(context, "/", routeResponseService, routeService, processorsDeserializer, "GET", cache);
}
);
app.MapGet("/{*path}", async (
	HttpContext context,
	IMemoryCache cache,
	[FromRoute] string path,
	[FromServices] IRouteResponseService routeResponseService,
	[FromServices] IRouteService routeService,
	[FromServices] IProcessorsDeserializer processorsDeserializer) =>
{
	return await HttpRouteHandler.Handler(context, path, routeResponseService, routeService, processorsDeserializer, "GET", cache);
}
);

// Post Handler

app.MapPost("/", async (
	HttpContext context,
	IMemoryCache cache,
	[FromServices] IRouteResponseService routeResponseService,
	[FromServices] IRouteService routeService,
	[FromServices] IProcessorsDeserializer processorsDeserializer) =>
{
	return await HttpRouteHandler.Handler(context, "/", routeResponseService, routeService, processorsDeserializer, "POST", cache);
}
);
app.MapPost("/{*path}", async (
	HttpContext context,
	IMemoryCache cache,
	[FromRoute] string path,
	[FromServices] IRouteResponseService routeResponseService,
	[FromServices] IRouteService routeService,
	[FromServices] IProcessorsDeserializer processorsDeserializer) =>
{
	return await HttpRouteHandler.Handler(context, path, routeResponseService, routeService, processorsDeserializer, "POST", cache);
}
);

// Put Handler

app.MapPut("/", async (
	HttpContext context,
	IMemoryCache cache,
	[FromServices] IRouteResponseService routeResponseService,
	[FromServices] IRouteService routeService,
	[FromServices] IProcessorsDeserializer processorsDeserializer) =>
{
	return await HttpRouteHandler.Handler(context, "/", routeResponseService, routeService, processorsDeserializer, "PUT", cache);
}
);
app.MapPut("/{*path}", async (
	HttpContext context,
	IMemoryCache cache,
	[FromRoute] string path,
	[FromServices] IRouteResponseService routeResponseService,
	[FromServices] IRouteService routeService,
	[FromServices] IProcessorsDeserializer processorsDeserializer) =>
{
	return await HttpRouteHandler.Handler(context, path, routeResponseService, routeService, processorsDeserializer, "PUT", cache);
}
);

// Delete Handler

app.MapDelete("/", async (
	HttpContext context,
	IMemoryCache cache,
	[FromServices] IRouteResponseService routeResponseService,
	[FromServices] IRouteService routeService,
	[FromServices] IProcessorsDeserializer processorsDeserializer) =>
{
	return await HttpRouteHandler.Handler(context, "/", routeResponseService, routeService, processorsDeserializer, "DELETE", cache);
}
);
app.MapDelete("/{*path}", async (
	HttpContext context,
	IMemoryCache cache,
	[FromRoute] string path,
	[FromServices] IRouteResponseService routeResponseService,
	[FromServices] IRouteService routeService,
	[FromServices] IProcessorsDeserializer processorsDeserializer) =>
{
	return await HttpRouteHandler.Handler(context, path, routeResponseService, routeService, processorsDeserializer, "DELETE", cache);
}
);

app.Run();