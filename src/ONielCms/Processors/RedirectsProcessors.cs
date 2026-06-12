using OnielCms.Core;
using static OnielCms.Core.HttpRouteHandler;

namespace ONielCms.Processors
{

    public static class RedirectsProcessors
    {

        public static ValueTask TemporaryRedirect(ref ProcessorState state, ProcessorElement processorElement)
        {
            string redirectUrl = GetRedirectUrl(processorElement);

            state.Result = Results.Redirect(redirectUrl, false, true);
            state.Handled = true;
            return ProcessorsShared.EmptyValueTask;
        }

        public static ValueTask TemporaryRedirectWithoutBody(ref ProcessorState state, ProcessorElement processorElement)
        {
            string redirectUrl = GetRedirectUrl(processorElement);

            state.HttpContext.Response.Redirect(redirectUrl, false, false);
            state.Handled = true;
            return ProcessorsShared.EmptyValueTask;
        }

        private static string GetRedirectUrl(ProcessorElement processorElement)
        {
            var redirectUrl = processorElement.Parameters.FirstOrDefault(a => a.Name == "Url")?.Value ?? "";
            if (string.IsNullOrEmpty(redirectUrl)) throw new Exception("TemporaryRedirect: Url parameter is required to perform this action!");
            return redirectUrl;
        }

    }

}
