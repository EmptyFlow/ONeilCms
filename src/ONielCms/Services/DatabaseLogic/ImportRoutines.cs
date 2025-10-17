using Microsoft.AspNetCore.StaticFiles;

namespace ONielCms.Services.DatabaseLogic {

    public static class ImportRoutines {

        public static bool IsDownloadbleFile ( string mimeType ) {
            switch ( mimeType.ToLowerInvariant () ) {
                case "text/css":
                case "text/html":
                case "text/plain":
                case "text/javascript":
                case "text/markdown":
                case "image/jpeg":
                case "image/png":
                case "image/gif":
                case "image/svg+xml":
                case "image/webp":
                case "application/json":
                case "application/xml":
                    return false;
            }

            return true;
        }

        public static string GetMimeTypeForFileExtension ( string filePath ) {
            const string DefaultContentType = "application/octet-stream";

            var provider = new FileExtensionContentTypeProvider ();

            if ( !provider.TryGetContentType ( filePath, out var contentType ) ) {
                contentType = DefaultContentType;
            }

            return contentType;
        }

        public static string GetMethodName ( string method ) => method.ToLowerInvariant () switch {
            "get" => "GET",
            "post" => "POST",
            "put" => "POST",
            "delete" => "DELETE",
            "patch" => "PATCH",
            _ => throw new NotSupportedException ( $"Method {method} not supported!" )
        };

    }

}
