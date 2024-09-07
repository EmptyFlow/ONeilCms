using ONielCommon.Storage;

namespace ONielCms.Models {

    [TableName("sitefile")]
    public record SiteFileModel {

        public byte[] Content { get; init; } = [];

        public string Version { get; set; } = "";

        public string Path { get; set; } = "";

    }

}
