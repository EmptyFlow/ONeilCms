namespace WebApplaudDeveloper {

	public static class GlobalConfig {

		private static string m_path = "";

		private static string m_datePath = "";

		public static string Path => m_path;

		public static string DataPath => m_datePath;

		public static void SetupPath(string path) => m_path = path.Replace('\\', '/');

		public static void SetupDataPath(string path) => m_datePath = path.Replace('\\', '/');

	}

}
