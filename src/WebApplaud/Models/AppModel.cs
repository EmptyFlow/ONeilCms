namespace WebApplaud.Models
{

	public class AppModel
	{

		public required string Id { get; set; } = "";

		public required string LocalId { get; set; } = "";

		public string Icon { get; set; } = "";

		public string ManifestUrl { get; set; } = "";

		public string DownloadUrl { get; set; } = "";

		public IEnumerable<string> Factors { get; set; } = Enumerable.Empty<string>();

		public IEnumerable<string> Developers { get; set; } = Enumerable.Empty<string>();

		public IDictionary<string, string> Titles { get; set; } = new Dictionary<string, string>();



	}

}
