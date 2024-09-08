namespace ONielCommon.Entities {

    public record ResourceContent {

        public Guid Id { get; init; }

        public string Content { get; set; } = "";

    }

}
