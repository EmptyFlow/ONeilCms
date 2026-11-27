using System.Text.Json.Serialization;
using WebApplaudDeveloper.Models;

namespace WebApplaudDeveloper
{

	[JsonSerializable(typeof(FolderItemsModel))]
	[JsonSerializable(typeof(AppManifestModel))]
	[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
	internal partial class AppJsonSerializerContext : JsonSerializerContext
	{
	}

}
