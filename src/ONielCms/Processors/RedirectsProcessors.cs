using OnielCms.Core;

namespace ONielCms.Processors
{

	public static class RedirectsProcessors
	{

		public static ValueTask<IResult> TemporaryRedirect(HttpContext httpContext, IEnumerable<ProcessorElementParameter> parameters)
		{
			string redirectUrl = GetRedirectUrl(parameters);

			return ValueTask.FromResult(Results.Redirect(redirectUrl, false, true));
		}

		public static ValueTask<IResult> TemporaryRedirectWithoutBody(HttpContext httpContext, IEnumerable<ProcessorElementParameter> parameters)
		{
			string redirectUrl = GetRedirectUrl(parameters);

			return ValueTask.FromResult(Results.Redirect(redirectUrl, false, false));
		}

		private static string GetRedirectUrl(IEnumerable<ProcessorElementParameter> parameters)
		{
			var redirectUrl = parameters.FirstOrDefault(a => a.Name == "Url")?.Value ?? "";
			if (string.IsNullOrEmpty(redirectUrl)) throw new Exception("TemporaryRedirect: Url parameter is required to perform this action!");
			return redirectUrl;
		}

	}

}
