namespace WebApplaud
{
	public static class GlobalConfig
	{

		private static string m_path = "";

		public static string Path => m_path;

		public static void SetupPath(string path) => m_path = path;

	}
}
