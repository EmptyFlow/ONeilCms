using static ONielCms.Handlers.SiteBodyHandler;

namespace ONielCms.Processors {

    public static class RedirectsProcessors {

        public static ValueTask TemporaryRedirect ( ref ProcessorState state ) {
            state.HttpContext.Response.Redirect ( "", false, true );
            return ProcessorsShared.EmptyValueTask;
        }

        public static ValueTask TemporaryRedirectWithoutBody ( ref ProcessorState state ) {
            state.HttpContext.Response.Redirect ( "", false, false );
            return ProcessorsShared.EmptyValueTask;
        }

    }

}
