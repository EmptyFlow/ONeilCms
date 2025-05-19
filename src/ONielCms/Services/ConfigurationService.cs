using ONielCms.Models;
using ONielCommon.Readers;
using ONielCommon.Storage;

namespace ONielCms.Services {

    /// <summary>
    /// Configuration service.
    /// </summary>
    public class ConfigurationService : IConfigurationService {

        private const string ConfigFileName = "oneil.config";

        private static ConfigurationModel? m_configurationModel;

        private static bool m_configurationLoaded = false;

        public static void Initialize () {
            Console.WriteLine ( "Configuration initialization started..." );

            var searchedPaths = new string[] {
                Path.GetFullPath ( ConfigFileName ),
                Path.Combine ( Environment.GetFolderPath ( Environment.SpecialFolder.ApplicationData ), ConfigFileName ),
                Path.Combine ( Environment.GetFolderPath ( Environment.SpecialFolder.LocalApplicationData ), ConfigFileName ),
                Path.Combine ( Environment.GetFolderPath ( Environment.SpecialFolder.CommonApplicationData ), ConfigFileName )
            };

            foreach ( string path in searchedPaths ) {
                if ( !File.Exists ( path ) ) continue;

                Console.WriteLine ( $"Try read from {path}..." );

                var pairs = GetPairsFromFile ( path );
                MapPairsToModel ( pairs );

                m_configurationLoaded = true;
                Console.WriteLine ( $"Configuration loaded" );
            }
        }

        private static IDictionary<string, string> GetPairsFromFile ( string fileName ) {
            try {
                var pairs = KeyValueReader.GetPairs ( File.ReadAllText ( fileName ) );

                return pairs;
            } catch ( Exception e ) {
                Console.WriteLine ( e.Message + "\n" + e.StackTrace );
                throw;
            }
        }

        private static void MapPairsToModel ( IDictionary<string, string> pairs ) {
            m_configurationModel = new ConfigurationModel {
                DatabaseConnectionString = pairs.ContainsKey ( "DatabaseConnectionString" ) ? pairs["DatabaseConnectionString"] : ""
            };
        }

        public bool ConfigurationLoaded () => m_configurationLoaded;

        public string DatabaseConnectionString () => m_configurationModel?.DatabaseConnectionString ?? "";

    }

}
