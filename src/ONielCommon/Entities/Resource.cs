using ONielCommon.Storage;

namespace ONielCommon.Entities {

    [TableName("resource")]
    public record Resource {

        public Guid Id { get; init; }

        public string Identifier { get; init; } = "";

    }

}
