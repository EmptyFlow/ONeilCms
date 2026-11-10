namespace WebApplaud.FileSystems {
	internal interface IDirectoryModel {

		string Name { get; }

		string FullName { get; }

		IEnumerable <IFileModel> EnumerateFiles();

		IEnumerable<IDirectoryModel> GetDirectories();

	}

}
