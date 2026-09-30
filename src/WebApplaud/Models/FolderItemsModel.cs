namespace WebApplaud.Models
{

	public sealed record FolderItemsModel
	{

		public required IEnumerable<FolderItemModel> Files { get; init; } = [];

		public required IEnumerable<FolderItemModel> Folders { get; init; } = [];

	}

}
