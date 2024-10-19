namespace ONielCommon.Entities {

    /// <summary>
    /// Website route.
    /// </summary>
    public record Route {

        public Guid Id { get; init; }

        public string Path { get; init; } = "";

        public string ContentType { get; init; } = "";

        public string Method { get; init; } = "";

    }

}
