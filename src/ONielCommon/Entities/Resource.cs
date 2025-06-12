using ONielCommon.Storage;

namespace ONielCommon.Entities {

    [TableName("resource")]
    public record Resource {

        public Guid Id { get; init; }

        public string Identifier { get; init; } = "";

        public byte[] Content { get; set; } = [];

        public string ContentHash { get; set; } = "";

    }

    [TableName ( "resource" )]
    public record ResourceWithoutContent {

        public Guid Id { get; init; }

        public string Identifier { get; init; } = "";

        public string ContentHash { get; set; } = "";

    }

}
