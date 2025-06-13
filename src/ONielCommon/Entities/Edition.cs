using ONielCommon.Storage;

namespace ONielCommon.Entities {

    [TableName("edition")]
    public class Edition {

        public Guid Id { get; init; }

        public string Version { get; init; } = "";

        public DateTime Created { get; set; }

    }

}
