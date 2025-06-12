using FlowCommandLine;
using ONielCms.Services.DatabaseLogic;
using ONielCommon.Storage;

namespace ONielCms.Services {

    public class ImportCommand {

        public string Path { get; set; } = "";

    }

    public static class CommandLineHandler {

        public static async Task<bool> HandleCommandLine ( IStorageContext storageContext ) {
            var result = await CommandLine.Console ()
                .Application ( "OnielCms", "1.0.1", "Web content management system.", "Copyright (c) 2025 Roman Vladimirov", "onielcms" )
                .AddAsyncCommand (
                    "import-file",
                    async ( ImportCommand parameters ) => {
                        var service = new ImportVersionService ( storageContext );
                        await service.ImportFromFile ( parameters.Path );
                    },
                    "Import new version from specified folder",
                    new List<FlowCommandParameter> {
                        FlowCommandParameter.CreateRequired(name: "p", alias: "path", help: "Path to folder where containing new version"),
                    }
                )
                .RunCommandAsync();

            return !result.EmptyInput && ( result.CommandHandled || result.Handled );
        }

    }

}
