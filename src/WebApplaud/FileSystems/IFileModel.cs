namespace WebApplaud.FileSystems {
	internal interface IFileModel {

		string DirectoryName { get; set; }

		string Extension { get; set; }

		string Name { get; set; }

		DateTime LastWriteTimeUtc { get; set; }

		string FullName { get; set; }

	}

}
