namespace WebApplaud.FileSystems {
	internal sealed record DirectoryModel : IDirectoryModel {

		private readonly DirectoryInfo m_directoryInfo;

		public DirectoryModel(DirectoryInfo directoryInfo) {
			m_directoryInfo = directoryInfo;
		}

		public string Name => m_directoryInfo.Name;

		public string FullName => m_directoryInfo.FullName;

		public IEnumerable<IFileModel> EnumerateFiles() {
			var files = m_directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories);
			return files
				.Select(
					a => new FileModel {
						Name = a.Name,
						Extension = a.Extension,
						DirectoryName = a.DirectoryName ?? "",
						FullName = a.FullName,
						LastWriteTimeUtc = a.LastWriteTimeUtc
					}
				)
				.ToList();
		}

		public IEnumerable<IDirectoryModel> GetDirectories() {
			var directories = m_directoryInfo.GetDirectories();

			return directories
				.Select(a => new DirectoryModel(a))
				.ToList();
		}

	}

}
