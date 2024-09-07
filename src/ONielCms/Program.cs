using ONielCms.Handlers;
using ONielCms.Services;

ConfigurationService.Initialize ();

var builder = WebApplication.CreateSlimBuilder ( args );

var app = builder.Build ();

var routeBuilder = app.MapGroup ( "/site" );
SiteHandler.Register ( routeBuilder );

app.Run ();

/*
builder.Services.ConfigureHttpJsonOptions ( options => {
    options.SerializerOptions.TypeInfoResolverChain.Insert ( ^0, AppJsonSerializerContext.Default );
} );
var sampleTodos = new Todo[] {
    new(1, "Walk the dog"),
    new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
    new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
    new(4, "Clean the bathroom"),
    new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
};
public record Todo ( int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false );

[JsonSerializable ( typeof ( Todo[] ) )]
internal partial class AppJsonSerializerContext : JsonSerializerContext {

}*/