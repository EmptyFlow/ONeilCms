using System.Reflection;
using System.Text;

namespace WebApplaud.Pages
{

	public static class LoginPage
	{

		private static byte[] GetEmbeddedResources(string resource)
		{
			var assembly = Assembly.GetExecutingAssembly();
			var defaultNamespace = assembly.GetName().Name ?? "";
			var prefix = $"{defaultNamespace}.Pages.";

			var dict = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

			using var stream = assembly.GetManifestResourceStream(prefix + resource);
			if (stream == null) return [];

			using var ms = new MemoryStream();
			stream.CopyTo(ms);
			return ms.ToArray();
		}

		private static byte[] m_drawerTemplate = [];

		private static byte[] m_robotoTemplate = [];

		public static void Initialize()
		{
			m_drawerTemplate = GetEmbeddedResources("LoginPage.html");
			m_robotoTemplate = GetEmbeddedResources("Roboto.ttf");
		}

		public static void RegisterRoutes(WebApplication app, bool maskRoutes = false)
		{
			app.MapGet($"/", (HttpContext httpContext) =>
			{
				return Results.Bytes(m_drawerTemplate, "text/html");
			});
			app.MapGet($"/Roboto.ttf", (HttpContext httpContext) =>
			{
				return Results.Bytes(m_robotoTemplate, "text/html");
			});
		}

		//Content-Security-Policy: base-uri 'self'; default-src 'self'; object-src 'none'; script-src 'self'; style-src *; img-src *;
		//connect-src https://host1.com https://host2.com

	}
}
