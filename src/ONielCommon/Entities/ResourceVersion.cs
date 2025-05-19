using ONielCommon.Storage;

namespace ONielCommon.Entities {

    [TableName ( "resourceversion" )]
    public class ResourceVersion {

        public Guid Id { get; init; }

        public string Version { get; set; } = "";

        public Guid ResourceId { get; set; }

    }

}
