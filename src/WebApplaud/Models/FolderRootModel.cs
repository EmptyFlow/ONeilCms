namespace WebApplaud.Models
{

	public class FolderRootModel
	{

		public string Path { get; set; } = "";

		public Dictionary<string, string> Files { get; set; } = new Dictionary<string, string>();

		public Dictionary<string, string> Folders { get; set; } = new Dictionary<string, string>();

	}

}
