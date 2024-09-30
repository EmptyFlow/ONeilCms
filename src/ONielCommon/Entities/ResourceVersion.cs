namespace ONielCommon.Entities {

    public class ResourceVersion {

        public Guid Id { get; init; }

        public string Edition { get; set; } = "";

        public Guid ResourceId { get; set; }

        public Guid ResourceContentId { get; set; }

    }

}
