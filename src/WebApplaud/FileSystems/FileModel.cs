namespace WebApplaud.FileSystems {

	internal sealed record FileModel : IFileModel {

		public string DirectoryName { get; set; } = "";

		public string Extension { get; set; } = "";

		public string Name { get; set; } = "";

		public DateTime LastWriteTimeUtc { get; set; }

		public string FullName { get; set; } = "";

	}

}
