namespace ONielCms.Models {

    public record ImportVersionModelResource {

        public string Id { get; init; } = "";

        public string? RawContent { get; set; }

        public string? FileContent { get; set; }

        public string? Base64Content { get; set; }

    }

}