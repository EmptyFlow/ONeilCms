using FlowCommandLine;
using WebApplaudDeveloper.Models;

namespace WebApplaudDeveloper
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
					.Application("WebApplaudDeveloper", fileVersion, "Application for developing and testing WebApps.", "(c) 2026 Roman Vladimirov. All rights reserved.", "webapplauddeveloper")
					.AddAsyncCommand(
						"run",
						async (StartupOptionsModel parameters) =>
						{
							Console.WriteLine("Run in developing mode");
							Console.WriteLine("Application path: " + parameters.AppsPath);
							Console.WriteLine("Content path: " + parameters.DataPath);
							GlobalConfig.SetupPath(parameters.AppsPath);
							GlobalConfig.SetupDataPath(parameters.DataPath);



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
					.RunCommandAsync();
				return result.CommandHandled || result.Failed;
			}

			return false;
		}

	}
}
