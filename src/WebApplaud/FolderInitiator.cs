namespace WebApplaud
{
	public static class FolderInitiator
	{

		private static Dictionary<string, string> m_rootFolders = new()
		{
			["apps"] = "Application",
			["data"] = "Data"
		};

		private static Dictionary<string, string> m_dataFolders = new()
		{
			["users"] = "User's folders",
			["common"] = "Shared folder"
		};

		public static bool FoldersNotExists(string path)
		{
			foreach (var folder in m_rootFolders.Keys)
			{
				if (!Directory.Exists(Path.Combine(path, folder))) return true;
			}

			return false;
		}

		public static void CreateFolderStructure(string path)
		{
			GlobalLogger.Information($"Initialize process", "FolderInitiator.CreateFolderStructure");
			if (!Directory.Exists(path))
			{
				GlobalLogger.Error($"Can't create folder structure, path '{path}' is not exists or not allowable!", "FolderInitiator.CreateFolderStructure");
				return;
			}

			// create apps directory
			GlobalLogger.Information($"Create Applications directory", "FolderInitiator.CreateFolderStructure");

			foreach (var folder in m_rootFolders)
			{
				var created = CreateDirectory(Path.Combine(path, folder.Key), folder.Value);
				if (!created) throw new Exception("Can't create folder structure!");
			}

			foreach (var dataFolder in m_dataFolders)
			{
				var created = CreateDirectory(Path.Combine(path, "data", dataFolder.Key), dataFolder.Value);
				if (!created) throw new Exception("Can't create folder structure!");
			}
		}

		private static bool CreateDirectory(string path, string name)
		{
			if (Directory.Exists(path)) return true;

			GlobalLogger.Information($"Create {name} directory", "FolderInitiator.CreateDirectory");
			try
			{
				Directory.CreateDirectory(path);
			}
			catch (Exception exception)
			{
				GlobalLogger.Error($"Failed to create {name} directory: {exception.Message}", "FolderInitiator.CreateDirectory");
				return false;
			}

			return true;
		}

	}
}
