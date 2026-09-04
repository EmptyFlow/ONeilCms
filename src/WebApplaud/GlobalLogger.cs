namespace WebApplaud
{

	public static class GlobalLogger
	{

		public static void Information(string message, string? module = default)
		{
			Console.WriteLine(message);
		}

		public static void Error(string message, string? module = default)
		{
			Console.WriteLine(message);
		}

	}

}
