using Microsoft.AspNetCore.Mvc;
using ONielCms;
using ONielCms.Handlers;
using ONielCms.Services;
using ONielCms.Services.DatabaseLogic;
using ONielCommon.Storage;
using ONielCommon.Storage.EntityServices;

ConfigurationService.Initialize ();
var storageService = new StorageContext ( new ConsoleStorageLogger (), new ConfigurationService () );

if ( await CommandLineHandler.HandleCommandLine ( storageService ) ) return;

Console.WriteLine ( "No commands passed, start to server" );

var builder = WebApplication.CreateBuilder ( Enumerable.Empty<string> ().ToArray () );

Dependencies.Resolve ( builder.Services );

var app = builder.Build ();

//app.Urls.Add("http://localhost:4000");

app.UseRouting ();

// Get Handler

app.MapGet ( "/", async (
    HttpContext context,
    [FromServices] IRouteResponseService routeResponseService,
    [FromServices] IRouteService routeService ) => {
        return await SiteGetHandler.Handler ( context, "/", routeResponseService, routeService );
    }
);
app.MapGet ( "/{*path}", async (
    HttpContext context,
    [FromRoute] string path,
    [FromServices] IRouteResponseService routeResponseService,
    [FromServices] IRouteService routeService ) => {
        return await SiteGetHandler.Handler ( context, path, routeResponseService, routeService );
    }
);

// Post Handler

app.MapPost ( "/", async (
    HttpContext context,
    [FromServices] IRouteResponseService routeResponseService,
    [FromServices] IRouteService routeService ) => {
        return await SitePostHandler.Handler ( context, "/", routeResponseService, routeService );
    }
);
app.MapPost ( "/{*path}", async (
    HttpContext context,
    [FromRoute] string path,
    [FromServices] IRouteResponseService routeResponseService,
    [FromServices] IRouteService routeService ) => {
        return await SitePostHandler.Handler ( context, path, routeResponseService, routeService );
    }
);

// Put Handler

app.MapPut ( "/", async (
    HttpContext context,
    [FromServices] IRouteResponseService routeResponseService,
    [FromServices] IRouteService routeService ) => {
        return await SitePutHandler.Handler ( context, "/", routeResponseService, routeService );
    }
);
app.MapPut ( "/{*path}", async (
    HttpContext context,
    [FromRoute] string path,
    [FromServices] IRouteResponseService routeResponseService,
    [FromServices] IRouteService routeService ) => {
        return await SitePutHandler.Handler ( context, path, routeResponseService, routeService );
    }
);

// Delete Handler

app.MapDelete ( "/", async (
    HttpContext context,
    [FromServices] IRouteResponseService routeResponseService,
    [FromServices] IRouteService routeService ) => {
        return await SiteDeleteHandler.Handler ( context, "/", routeResponseService, routeService );
    }
);
app.MapDelete ( "/{*path}", async (
    HttpContext context,
    [FromRoute] string path,
    [FromServices] IRouteResponseService routeResponseService,
    [FromServices] IRouteService routeService ) => {
        return await SiteDeleteHandler.Handler ( context, path, routeResponseService, routeService );
    }
);

app.Run ();