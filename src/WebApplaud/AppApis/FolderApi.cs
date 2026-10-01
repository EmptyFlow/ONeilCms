using Microsoft.AspNetCore.Mvc;
using WebApplaud.Models;

namespace WebApplaud.AppApis
{

	public static class FolderApi
	{

		public static void RegisterRoutes(WebApplication app, Dictionary<string, FolderRootModel> folderRoots, bool maskRoutes = false)
		{
			app.MapGet($"folderroots", () =>
			{
				if (folderRoots.Count != 0)
				{
					var roots = string.Join(',', folderRoots.Keys.Select(a => $"\"{a}\""));
					return Results.Content($"[{roots}]", contentType: "application/json");
				}
				else
				{
					return Results.Content("[]", contentType: "application/json");
				}
			});
			app.MapGet($"folderitems", ([FromQuery] string rootId, [FromQuery] string? id = default) =>
			{
				if (folderRoots.TryGetValue(rootId, out var rootItem))
				{
					var rootPath = rootItem.Path + "/";
					//get root common folder
					if (string.IsNullOrEmpty(id))
					{
						var files = rootItem.Files
							.Where(a => a.Value.Replace(rootPath, "").IndexOf("/") == -1)
							.Select(a => new FolderItemModel { Name = a.Value.Replace(rootPath, ""), Id = a.Key })
							.ToArray();
						var folders = rootItem.Folders
							.Where(a => a.Value.Replace(rootPath, "").IndexOf("/") == -1)
							.Select(a => new FolderItemModel { Name = a.Value.Replace(rootPath, ""), Id = a.Key })
							.ToArray();
						return Results.Json(new FolderItemsModel { Files = files, Folders = folders }, AppJsonSerializerContext.Default);
					}
					else
					{
						if (rootItem.Folders.TryGetValue(id, out var folderRoot))
						{
							var rootFolderPath = folderRoot + "/";
							var files = rootItem.Files
								.Where(a => a.Value.Replace(rootFolderPath, "").IndexOf("/") == -1)
								.Select(a => new FolderItemModel { Name = a.Value.Replace(rootFolderPath, ""), Id = a.Key })
								.ToArray();
							var folders = rootItem.Folders
								.Where(a => a.Value.Replace(rootFolderPath, "").IndexOf("/") == -1)
								.Select(a => new FolderItemModel { Name = a.Value.Replace(rootFolderPath, ""), Id = a.Key })
								.ToArray();
							return Results.Json(new FolderItemsModel { Files = files, Folders = folders }, AppJsonSerializerContext.Default);
						}
					}
				}

				return Results.NotFound();
			});
			app.MapGet($"file", ([FromQuery] string rootId, [FromQuery] string id) =>
			{
				if (folderRoots.TryGetValue(rootId, out var rootItem))
				{
					if (rootItem.Files.TryGetValue(id, out var fileItem))
					{
						var mimeType = FolderInitiator.GetMimeTypeForFileExtension(fileItem);
						var fullPath = Path.Combine(GlobalConfig.Path, fileItem.StartsWith('/') ? fileItem.Substring(1) : fileItem);
						var fileName = Path.GetFileName(fullPath);
						if (!File.Exists(fullPath)) return Results.NotFound();

						var stream = File.OpenRead(fullPath);
						return Results.File(stream, contentType: mimeType, enableRangeProcessing: true, fileDownloadName: fileName);
					}
				}

				return Results.NotFound();
			});
			app.MapPost($"settextcontext", async ([FromQuery] string rootId, [FromQuery] string id, [FromBody] string content, CancellationToken cancellationToken) => {
				if (folderRoots.TryGetValue(rootId, out var rootItem))
				{
					if (rootItem.Files.TryGetValue(id, out var fileItem))
					{
						var fullPath = Path.Combine(GlobalConfig.Path, fileItem.StartsWith('/') ? fileItem.Substring(1) : fileItem);
						if (!File.Exists(fullPath)) return Results.Content("false", contentType: "application/json");

						await File.WriteAllTextAsync(fullPath, content, cancellationToken);
						return Results.Content("true", contentType: "application/json");
					}
				}

				return Results.Content("false", contentType: "application/json");
			});
		}

	}

}
