namespace WebApplaud.Pages
{

	public static class AppDrawer
	{

		public static void RegisterRoutes(WebApplication app, bool maskRoutes = false)
		{
			app.MapGet($"/", () =>
			{
				return Results.Content("", "text/html");
			});
		}

	}
}
