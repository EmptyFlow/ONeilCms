using Microsoft.AspNetCore.Mvc;
using WebApplaudDeveloper.Models;

namespace WebApplaudDeveloper.ApplicationApis
{

	public static class FolderApi
	{

		public static void RegisterRoutes(WebApplication app, HashSet<string> folderRoots)
		{
			app.MapGet($"folderroots", (HttpContext httpContext) =>
			{
				if (folderRoots.Count != 0)
				{
					var roots = string.Join(',', folderRoots.Select(a => $"\"{a}\""));
					return Results.Content($"[{roots}]", contentType: "application/json");
				}
				else
				{
					return Results.Content("[]", contentType: "application/json");
				}
			});
			app.MapGet($"folderitems", (HttpContext httpContext, [FromQuery] string rootId, [FromQuery] string? id = default) =>
			{
				if (folderRoots.TryGetValue(rootId, out var rootItem))
				{
					var rootPath = rootItem + "/";

					//get root common folder
					if (string.IsNullOrEmpty(id))
					{
						var directory = new DirectoryInfo(rootPath);
						var files = directory.GetFiles()
							.Select(a => new FolderItemModel { Name = a.Name, Id = a.FullName.Replace(rootPath, "") })
							.ToArray();
						var folders = directory.GetDirectories()
							.Select(a => new FolderItemModel { Name = a.Name, Id = a.FullName.Replace(rootPath, "") })
							.ToArray();
						return Results.Json(new FolderItemsModel { Files = files, Folders = folders }, AppJsonSerializerContext.Default);
					}
					else
					{
						var subPath = Path.Combine(rootPath, id);
						if (!Directory.Exists(subPath)) Results.NotFound();

						var directory = new DirectoryInfo(subPath);
						var files = directory.GetFiles()
							.Select(a => new FolderItemModel { Name = a.Name, Id = a.FullName.Replace(rootPath, "") })
							.ToArray();
						var folders = directory.GetDirectories()
							.Select(a => new FolderItemModel { Name = a.Name, Id = a.FullName.Replace(rootPath, "") })
							.ToArray();
						return Results.Json(new FolderItemsModel { Files = files, Folders = folders }, AppJsonSerializerContext.Default);
					}
				}

				return Results.NotFound();
			});
			app.MapGet($"file", (HttpContext httpContext, [FromQuery] string rootId, [FromQuery] string id) =>
			{
				if (folderRoots.TryGetValue(rootId, out var rootItem))
				{
					var filePath = Path.Combine(rootItem, id);
					if (!File.Exists(filePath)) return Results.NotFound();

					var mimeType = FolderInitiator.GetMimeTypeForFileExtension(filePath);
					var fileName = Path.GetFileName(filePath);

					var stream = File.OpenRead(filePath);
					return Results.File(stream, contentType: mimeType, enableRangeProcessing: true, fileDownloadName: fileName);
				}

				return Results.NotFound();
			});
			app.MapPost($"settextcontext", async (HttpContext httpContext, [FromQuery] string rootId, [FromQuery] string id, [FromBody] string content, CancellationToken cancellationToken) => {
				if (folderRoots.TryGetValue(rootId, out var rootItem)) {
					var filePath = Path.Combine(rootItem, id);
					if (!File.Exists(filePath)) return Results.NotFound();

					await File.WriteAllTextAsync(filePath, content, cancellationToken);
					return Results.Content("true", contentType: "application/json");
				}

				return Results.Content("false", contentType: "application/json");
			});
		}

	}

}
