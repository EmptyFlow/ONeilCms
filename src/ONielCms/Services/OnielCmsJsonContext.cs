using OnielCms.Core;
using ONielCms.Models;
using System.Text.Json.Serialization;

namespace ONielCms.Services {

    [JsonSerializable(typeof(ImportVersionModel))]
    [JsonSerializable(typeof(IEnumerable<ProcessorElement>))]
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    public partial class OnielCmsJsonContext : JsonSerializerContext {
    }

}
