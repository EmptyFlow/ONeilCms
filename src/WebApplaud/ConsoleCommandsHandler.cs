using FlowCommandLine;
using WebApplaud.InnerApis;
using WebApplaud.Models;

namespace WebApplaud
{

	internal record InitFoldersModel
	{
		public string BasePath { get; init; } = "";
	}

	public static class ConsoleCommandsHandler
	{

		public static async Task<bool> Handle(string fileVersion)
		{
			if (Environment.GetCommandLineArgs().Length > 1)
			{
				var result = await CommandLine.Console()
					.Application("WebApplaud", fileVersion, "Hub for install and run WebApps.", "(c) 2026 Roman Vladimirov. All rights reserved.", "webapplaud")
					.AddAsyncCommand(
						"createuser",
						async (CreateUserModel parameters) =>
						{
							Console.WriteLine("Start create user profile...");
							var profile = new UserProfileModel
							{
								Id = "",
								FirstName = parameters.FirstName,
								LastName = parameters.LastName,
								Login = parameters.Login,
								Password = parameters.Password,
								Role = parameters.Role,
							};
							await UserProfile.Initialize(onlyProfiles: true);
							try
							{
								await UserProfile.CreateUserProfile(profile);
							}
							catch (Exception exception)
							{
								Console.WriteLine("Can't create user profile: " + exception.Message);
							}
							Console.ForegroundColor = ConsoleColor.Green;
							Console.WriteLine("User profile successfully created!");
							Console.ResetColor();
						},
						"Create new user profile",
						[
							FlowCommandParameter.CreateRequired("fn", "firstname", "User First name"),
							FlowCommandParameter.CreateRequired("ln", "lastname", "User Last name"),
							FlowCommandParameter.CreateRequired("lg", "login", "User login"),
							FlowCommandParameter.CreateRequired("ps", "password", "User password"),
							FlowCommandParameter.Create("rl", "role", "User role"),
						]
					)
					.AddAsyncCommand(
						"initfolders",
						async (InitFoldersModel parameters) =>
						{
							Console.WriteLine("Start create folder structure...");
							if (FolderInitiator.FoldersNotExists(parameters.BasePath))
							{
								try
								{
									FolderInitiator.CreateFolderStructure(parameters.BasePath);
								}
								catch (Exception exception)
								{
									Console.WriteLine("Can't create folder structure: " + exception.Message);
								}
								Console.ForegroundColor = ConsoleColor.Green;
								Console.WriteLine("Folder structure successfully created!");
								Console.ResetColor();
								return;
							}
							Console.WriteLine("Folder structure already created!");
						},
						"Create required folder structure in specified folder",
						[
							FlowCommandParameter.CreateRequired("bp", "basepath", "Base path for folder structure")
						]
					)
					.RunCommandAsync();
				return result.CommandHandled;
			}

			return false;
		}

	}
}
