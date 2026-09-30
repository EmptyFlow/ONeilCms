using System.Text.Json.Serialization;
using WebApplaud.Models;

namespace WebApplaud
{

	[JsonSerializable(typeof(FolderItemsModel))]
	[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
	internal partial class AppJsonSerializerContext : JsonSerializerContext
	{
	}

}
