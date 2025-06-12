using FlowCommandLine;
using ONielCms.Services.DatabaseLogic;
using ONielCommon.Storage;

namespace ONielCms.Services {

    public class ImportCommand {

        public string Path { get; set; } = "";

    }

    public static class CommandLineHandler {

        public static bool HandleCommandLine ( IStorageContext storageContext ) {
            var result = CommandLine.Console ()
                .Application ( "OnielCms", "1.0.1", "Web content management system.", "Copyright (c) 2025 Roman Vladimirov", "onielcms" )
                .AddCommand (
                    "import-folder",
                    ( ImportCommand parameters ) => {
                        var service = new ImportVersionService ( storageContext );
                        service.ImportFromFile ( parameters.Path );
                    },
                    "Import new version from specified folder",
                    new List<FlowCommandParameter> {
                        FlowCommandParameter.CreateRequired(name: "p", alias: "path", help: "Path to folder where containing new version"),
                        FlowCommandParameter.Create(alias: "deletecurrent", help: "Need to delete current version if it was already installed"),
                    }
                )
                .RunCommand ();

            return result.CommandHandled || result.Handled;
        }

    }

}
