using System.Reflection;
using System.Text;

namespace WebApplaud.Pages
{

	public static class AppDrawer
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

		private static string m_appsJson = "";

		public static void Initialize() {
			m_drawerTemplate = GetEmbeddedResources("AppDrawer.html");
			m_appsJson = "";
		}

		public static void RegisterRoutes(WebApplication app, bool maskRoutes = false)
		{
			app.MapGet($"/", () =>
			{
				var content = Encoding.UTF8.GetString(m_drawerTemplate);

				content = content
					.Replace("let appsData = '';", $"let appsData = '{m_appsJson}';")
					.Replace("<appsjson></appsjson>", $"<appsjson>{m_appsJson}</appsjson>");

				return Results.Content(content, "text/html");
			});
		}

		//Content-Security-Policy: base-uri 'self'; default-src 'self'; object-src 'none'; script-src 'self'; style-src *; img-src *; upgrade-insecure-requests 'self';
		//connect-src https://host1.com https://host2.com

	}
}
