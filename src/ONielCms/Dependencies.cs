using ONielCms.Services;
using ONielCommon.Storage;
using ONielCommon.Storage.EntityServices;

namespace ONielCms {

    /// <summary>
    /// Resolve dependencies.
    /// </summary>
    public static class Dependencies {

        public static void Resolve ( IServiceCollection collection ) {
            collection.AddSingleton<IConfigurationService, ConfigurationService> ();
            collection.AddScoped<IStorageContext, StorageContext> ();
            collection.AddScoped<IRouteService, RouteService> ();
        }

    }

}
