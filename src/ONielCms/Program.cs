using Microsoft.AspNetCore.Mvc;
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

Dependencies.Resolve ( builder.Services );

var app = builder.Build ();

var configurationService = app.Services.GetService<IConfigurationService> ();
if ( configurationService != null ) await SiteBodyHandler.LoadAllRoutesInCurrentVersion ( configurationService );

//app.Urls.Add("http://localhost:4000");

app.UseRouting ();

// Get Handler

app.MapGet ( "/", async (
    HttpContext context,
    [FromServices] IRouteResponseService routeResponseService,
    [FromServices] IRouteService routeService ) => {
        return await SiteBodyHandler.Handler ( context, "/", routeResponseService, routeService, "GET" );
    }
);
app.MapGet ( "/{*path}", async (
    HttpContext context,
    [FromRoute] string path,
    [FromServices] IRouteResponseService routeResponseService,
    [FromServices] IRouteService routeService ) => {
        return await SiteBodyHandler.Handler ( context, path, routeResponseService, routeService, "GET" );
    }
);

// Post Handler

app.MapPost ( "/", async (
    HttpContext context,
    [FromServices] IRouteResponseService routeResponseService,
    [FromServices] IRouteService routeService ) => {
        return await SiteBodyHandler.Handler ( context, "/", routeResponseService, routeService, "POST" );
    }
);
app.MapPost ( "/{*path}", async (
    HttpContext context,
    [FromRoute] string path,
    [FromServices] IRouteResponseService routeResponseService,
    [FromServices] IRouteService routeService ) => {
        return await SiteBodyHandler.Handler ( context, path, routeResponseService, routeService, "POST" );
    }
);

// Put Handler

app.MapPut ( "/", async (
    HttpContext context,
    [FromServices] IRouteResponseService routeResponseService,
    [FromServices] IRouteService routeService ) => {
        return await SiteBodyHandler.Handler ( context, "/", routeResponseService, routeService, "PUT" );
    }
);
app.MapPut ( "/{*path}", async (
    HttpContext context,
    [FromRoute] string path,
    [FromServices] IRouteResponseService routeResponseService,
    [FromServices] IRouteService routeService ) => {
        return await SiteBodyHandler.Handler ( context, path, routeResponseService, routeService, "PUT" );
    }
);

// Delete Handler

app.MapDelete ( "/", async (
    HttpContext context,
    [FromServices] IRouteResponseService routeResponseService,
    [FromServices] IRouteService routeService ) => {
        return await SiteBodyHandler.Handler ( context, "/", routeResponseService, routeService, "DELETE" );
    }
);
app.MapDelete ( "/{*path}", async (
    HttpContext context,
    [FromRoute] string path,
    [FromServices] IRouteResponseService routeResponseService,
    [FromServices] IRouteService routeService ) => {
        return await SiteBodyHandler.Handler ( context, path, routeResponseService, routeService, "DELETE" );
    }
);

app.Run ();