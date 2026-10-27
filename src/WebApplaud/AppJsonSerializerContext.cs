using System.Text.Json.Serialization;
using WebApplaud.Models;

namespace WebApplaud
{

	[JsonSerializable(typeof(FolderItemsModel))]
	[JsonSerializable(typeof(AppManifestModel))]
	[JsonSerializable(typeof(List<AppDrawerModel>))]
	[JsonSerializable(typeof(UserProfileModel))]
	[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
	internal partial class AppJsonSerializerContext : JsonSerializerContext
	{
	}

}
