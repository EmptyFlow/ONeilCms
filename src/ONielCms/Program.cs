using Microsoft.AspNetCore.Mvc;
using ONielCms;
using ONielCms.Handlers;
using ONielCms.Services;

ConfigurationService.Initialize ();

var builder = WebApplication.CreateSlimBuilder ( args );

var app = builder.Build ();

Dependencies.Resolve ( builder.Services );

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