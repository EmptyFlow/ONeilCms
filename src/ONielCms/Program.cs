using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ONielCms;
using ONielCms.Handlers;
using ONielCms.Services;
using ONielCms.Services.DatabaseLogic;
using ONielCommon.Storage;

ConfigurationService.Initialize ();
var storageService = new StorageContext ( new ConsoleStorageLogger (), new ConfigurationService () );

if ( await CommandLineHandler.HandleCommandLine ( storageService ) ) return;

Console.WriteLine ( "No commands passed, start to server" );

var builder = WebApplication.CreateBuilder ( Enumerable.Empty<string> ().ToArray () );

builder.Services.AddMemoryCache ();

Dependencies.Resolve ( builder.Services );

var app = builder.Build ();

var configurationService = app.Services.GetService<IConfigurationService> ();
if ( configurationService != null ) await SiteBodyHandler.LoadAllRoutesInCurrentVersion ( configurationService );

//app.Urls.Add("http://localhost:4000");

app.UseRouting ();

// Get Handler

app.MapGet ( "/", async (
    HttpContext context,
    IMemoryCache cache,
    [FromServices] IRouteResponseService routeResponseService,
    [FromServices] IRouteService routeService ) => {
        return await SiteBodyHandler.Handler ( context, "/", routeResponseService, routeService, "GET", cache );
    }
);
app.MapGet ( "/{*path}", async (
    HttpContext context,
    IMemoryCache cache,
    [FromRoute] string path,
    [FromServices] IRouteResponseService routeResponseService,
    [FromServices] IRouteService routeService ) => {
        return await SiteBodyHandler.Handler ( context, path, routeResponseService, routeService, "GET", cache );
    }
);

// Post Handler

app.MapPost ( "/", async (
    HttpContext context,
    IMemoryCache cache,
    [FromServices] IRouteResponseService routeResponseService,
    [FromServices] IRouteService routeService ) => {
        return await SiteBodyHandler.Handler ( context, "/", routeResponseService, routeService, "POST", cache );
    }
);
app.MapPost ( "/{*path}", async (
    HttpContext context,
    IMemoryCache cache,
    [FromRoute] string path,
    [FromServices] IRouteResponseService routeResponseService,
    [FromServices] IRouteService routeService ) => {
        return await SiteBodyHandler.Handler ( context, path, routeResponseService, routeService, "POST", cache );
    }
);

// Put Handler

app.MapPut ( "/", async (
    HttpContext context,
    IMemoryCache cache,
    [FromServices] IRouteResponseService routeResponseService,
    [FromServices] IRouteService routeService ) => {
        return await SiteBodyHandler.Handler ( context, "/", routeResponseService, routeService, "PUT", cache );
    }
);
app.MapPut ( "/{*path}", async (
    HttpContext context,
    IMemoryCache cache,
    [FromRoute] string path,
    [FromServices] IRouteResponseService routeResponseService,
    [FromServices] IRouteService routeService ) => {
        return await SiteBodyHandler.Handler ( context, path, routeResponseService, routeService, "PUT", cache );
    }
);

// Delete Handler

app.MapDelete ( "/", async (
    HttpContext context,
    IMemoryCache cache,
    [FromServices] IRouteResponseService routeResponseService,
    [FromServices] IRouteService routeService ) => {
        return await SiteBodyHandler.Handler ( context, "/", routeResponseService, routeService, "DELETE", cache );
    }
);
app.MapDelete ( "/{*path}", async (
    HttpContext context,
    IMemoryCache cache,
    [FromRoute] string path,
    [FromServices] IRouteResponseService routeResponseService,
    [FromServices] IRouteService routeService ) => {
        return await SiteBodyHandler.Handler ( context, path, routeResponseService, routeService, "DELETE", cache );
    }
);

app.Run ();