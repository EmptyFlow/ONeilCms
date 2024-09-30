namespace ONielCommon.Entities {

    public record ResourceContent {

        public Guid Id { get; init; }

        public byte[] Content { get; set; } = [];

    }

}
