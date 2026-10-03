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

		public static void Initialize() {
			m_drawerTemplate = GetEmbeddedResources("AppDrawer.html");
		}

		public static void RegisterRoutes(WebApplication app, bool maskRoutes = false)
		{
			app.MapGet($"/", () =>
			{
				var content = Encoding.UTF8.GetString(m_drawerTemplate);

				content = content
					.Replace("const appsData = '';", "")
					.Replace("<appsjson></appsjson>", "");

				return Results.Content(content, "text/html");
			});
		}

	}
}
