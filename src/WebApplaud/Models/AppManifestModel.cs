using System.Text.Json.Serialization;

namespace WebApplaud.Models
{

	internal record AppManifestModel
	{

		[JsonPropertyName("identifier")]
		public required string Id { get; init; } = "";

		[JsonPropertyName("appIcon")]
		public string Icon { get; init; } = "";

		[JsonPropertyName("manifestUrl")]
		public string ManifestUrl { get; init; } = "";

		[JsonPropertyName("downloadUrl")]
		public string DownloadUrl { get; init; } = "";

		[JsonPropertyName("factors")]
		public IEnumerable<string> Factors { get; init; } = Enumerable.Empty<string>();

		[JsonPropertyName("developers")]
		public IEnumerable<string> Developers { get; init; } = Enumerable.Empty<string>();

		[JsonPropertyName("title")]
		public IDictionary<string, string> Titles { get; init; } = new Dictionary<string, string>();

	}

}
