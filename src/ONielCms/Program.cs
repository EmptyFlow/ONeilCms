using Microsoft.AspNetCore.Mvc;
using ONielCms;
using ONielCms.Handlers;
using ONielCms.Services;
using ONielCommon.Storage;

ConfigurationService.Initialize ();
var storageService = new StorageContext ( new ConsoleStorageLogger (), new ConfigurationService () );

if ( await CommandLineHandler.HandleCommandLine ( storageService ) ) return;

var builder = WebApplication.CreateBuilder ( Enumerable.Empty<string> ().ToArray () );

Dependencies.Resolve ( builder.Services );


var app = builder.Build ();

//app.Urls.Add("http://localhost:4000");

var routeBuilder = app.MapGroup ( "/" );
routeBuilder.MapGet (
    "/{*path}",
    SiteHandler.GetHandler
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
);

app.Run ();