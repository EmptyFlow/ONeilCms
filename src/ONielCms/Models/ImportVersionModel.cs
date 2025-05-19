namespace ONielCms.Models {

    /// <summary>
    /// Import version model.
    /// </summary>
    public record ImportVersionModel {

        /// <summary>
        /// Related version data.
        /// </summary>
        public ImportVersionModelData Data { get; set; } = new ImportVersionModelData();

        /// <summary>
        /// List of routes.
        /// </summary>
        public IEnumerable<ImportVersionModelRoute> Routes { get; set; } = Enumerable.Empty<ImportVersionModelRoute>();

        /// <summary>
        /// List of resources.
        /// </summary>
        public IEnumerable<ImportVersionModelResource> Resources { get; set; } = Enumerable.Empty<ImportVersionModelResource> ();

    }

}
