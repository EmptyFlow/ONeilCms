using Microsoft.AspNetCore.Mvc;
using ONielCms;
using ONielCms.Handlers;
using ONielCms.Services;

ConfigurationService.Initialize ();

var builder = WebApplication.CreateBuilder ( args );

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

app.Run ();