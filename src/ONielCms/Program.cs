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

var builder = WebApplication.CreateBuilder ( Enumerable.Empty<string> ().ToArray () );

Dependencies.Resolve ( builder.Services );

var app = builder.Build ();

//app.Urls.Add("http://localhost:4000");

app.UseRouting ();

app.MapGet ( "/", async (
    HttpContext context,
    [FromServices] IRouteResponseService routeResponseService,
    [FromServices] IRouteService routeService ) => {
        return await SiteGetHandler.GetHandler ( "/", routeResponseService, routeService );
    }
);

app.MapGet ( "/{*path}", async (
    HttpContext context,
    [FromRoute] string path,
    [FromServices] IRouteResponseService routeResponseService,
    [FromServices] IRouteService routeService ) => {
        return await SiteGetHandler.GetHandler ( path, routeResponseService, routeService );
    }
);


/*var routeBuilder = app.MapGroup ( "/" );

routeBuilder.MapGet (
    "/{*path}",
    SiteGetHandler.GetHandler
);
routeBuilder.MapPost (
    "/{*path}",
    ( [FromRoute] string path, HttpContext context ) => {
        //context.Request.Body
        return Results.Ok ();
    }
);
routeBuilder.MapPut (
    "/{*path}",
    ( [FromRoute] string path, HttpContext context ) => {
        //context.Request.Body
        return Results.Ok ();
    }
);
routeBuilder.MapDelete (
    "/{*path}",
    ( [FromRoute] string path ) => {
        return Results.Ok ();
    }
);
routeBuilder.MapPatch (
    "/{*path}",
    ( [FromRoute] string path, HttpContext context ) => {
        return Results.Ok ();
    }
);*/

app.Run ();