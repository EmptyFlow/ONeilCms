namespace WebApplaud.FileSystems {
	internal interface IInternalFileSystem {

		IEnumerable<IDirectoryModel> GetDirectories(string path);

		bool FileExists(string path);

	}

}
