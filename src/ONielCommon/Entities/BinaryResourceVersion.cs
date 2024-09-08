namespace ONielCommon.Entities {

    public class BinaryResourceVersion {

        public Guid Id { get; init; }

        public string Edition { get; set; } = "";

        public Guid ResourceContentId { get; set; }

    }

}
