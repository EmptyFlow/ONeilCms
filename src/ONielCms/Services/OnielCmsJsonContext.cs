using ONielCms.Models;
using System.Text.Json.Serialization;

namespace ONielCms.Services {

    [JsonSerializable ( typeof ( ImportVersionModel ) )]
    [JsonSourceGenerationOptions ( PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase )]
    public partial class OnielCmsJsonContext : JsonSerializerContext {
    }

}
