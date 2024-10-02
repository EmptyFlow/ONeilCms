using ONielCommon.Storage;

namespace ONielCommon.Entities {

    [TableName ( "resourcecontent" )]
    public record ResourceContent {

        public Guid Id { get; init; }

        public byte[] Content { get; set; } = [];

    }

}
