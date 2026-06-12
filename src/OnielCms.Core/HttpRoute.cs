namespace OnielCms.Core
{
	public class HttpRoute
	{

		public Guid Id { get; init; }

		public string Path { get; init; } = "";

		public string ContentType { get; init; } = "";

		public string Method { get; init; } = "";

		public string DownloadFileName { get; set; } = "";

		public string Processors { get; set; } = "";

	}

}
