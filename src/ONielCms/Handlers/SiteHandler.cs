using Microsoft.AspNetCore.Mvc;
using ONielCommon.Storage;

namespace ONielCms.Handlers {

    public static class SiteHandler {

        public static void Register ( RouteGroupBuilder builder ) {
            builder.MapGet (
                "/{*path}",
                ( [FromRoute] string path ) => {
                    return Results.Ok ();
                }
            );
        }

        public static IResult Handler (
            [FromRoute] string path,
            [FromServices]IStorageContext storageContext ) {

            return Results.Ok ();
        }

    }

}
