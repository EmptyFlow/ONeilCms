namespace ONielCms.Models {

    /// <summary>
    /// Import route.
    /// </summary>
    public record ImportVersionModelRoute {

        /// <summary>
        /// Route path.
        /// </summary>
        public string Path { get; init; } = "";

        /// <summary>
        /// Content Type.
        /// </summary>
        public string ContentType { get; init; } = "";

        /// <summary>
        /// Route method.
        /// </summary>
        public string Method { get; init; } = "";

        /// <summary>
        /// Resources.
        /// </summary>
        public IEnumerable<string> Resources { get; init; } = Enumerable.Empty<string> ();

    }

}