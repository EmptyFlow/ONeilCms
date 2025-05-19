using FlowCommandLine;

namespace ONielCms.Services {

    public class ImportCommand {

        public string Path { get; set; } = "";

    }

    public static class CommandLineHandler {

        public static bool HandleCommandLine () {
            var result = CommandLine.Console ()
                .Application ( "OnielCms", "1.0.1", "Web content management system.", "Copyright (c) 2025 Roman Vladimirov", "onielcms" )
                .AddCommand (
                    "import-folder",
                    ( ImportCommand parameters ) => {
                    },
                    "Import new version from specified folder",
                    new List<FlowCommandParameter> {
                        FlowCommandParameter.CreateRequired(name: "p", alias: "path", help: "Path to folder where containing new version"),
                    }
                )
                .RunCommand ();

            return result.CommandHandled || result.Handled;
        }

    }

}
