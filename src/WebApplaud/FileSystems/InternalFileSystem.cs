namespace WebApplaud.FileSystems {
	internal sealed class InternalFileSystem : IInternalFileSystem {

		public IEnumerable<IDirectoryModel> GetDirectories(string path) {
			var directory = new DirectoryModel(new DirectoryInfo(path));
			return directory.GetDirectories();
		}

		public bool FileExists(string path) => File.Exists(path);

	}

}
