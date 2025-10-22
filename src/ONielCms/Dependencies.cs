using ONielCms.Services;
using ONielCms.Services.DatabaseLogic;
using ONielCommon.Storage;

namespace ONielCms {

    /// <summary>
    /// Resolve dependencies.
    /// </summary>
    public static class Dependencies {

        public static void Resolve ( IServiceCollection collection ) {
            collection.AddSingleton<IConfigurationService, ConfigurationService> ();
            collection.AddScoped<IStorageLogger, ConsoleStorageLogger> ();
            collection.AddScoped<IStorageContext, StorageContext> ();
            collection.AddScoped<IRouteService, RouteService> ();
            collection.AddScoped<IRouteResponseService, RouteResponseService> ();
        }

    }

}
