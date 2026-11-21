using System.Diagnostics;
using System.Text;

namespace WebApplaud.WebViewBinding {

	public class Webview : IDisposable {

		private bool m_disposed = false;
	
		private List<CallBackFunction> m_callbacks = new();

		private List<DispatchFunction> m_dispatchFunctions = new();

		private readonly nint m_nativeWebview;

		public Webview(bool debug = false, bool interceptExternalLinks = false) {
			m_nativeWebview = Bindings.webview_create(debug ? 1 : 0, nint.Zero);
			if (interceptExternalLinks) InterceptExternalLinks();
		}

		public void SetTitle(string title) {
			Bindings.webview_set_title(m_nativeWebview, title);
		}

		public void SetSize(int width, int height, WebviewHint hint) {
			Bindings.webview_set_size(m_nativeWebview, width, height, hint);
		}

		/// <summary>
		/// Injects JavaScript code at the initialization of the new page.
		/// </summary>
		public void InjectScriptBeforeLoadPages(string javascript) {
			Bindings.webview_init(m_nativeWebview, javascript);
		}

		public void OpenUrl(string url) {
			Bindings.webview_navigate(m_nativeWebview, url);
		}

		public void OpenHtml(string html) {
			var webviewUrl = new StringBuilder("data:text/html,");
			webviewUrl.Append(Uri.EscapeDataString(html));
			Bindings.webview_navigate(m_nativeWebview, webviewUrl.ToString());
		}

		public void Run() {
			Bindings.webview_run(m_nativeWebview);
		}

		public void Return(string id, RPCResult result, string resultJson) {
			Bindings.webview_return(m_nativeWebview, id, result, resultJson);
		}

		public void RunScript(string javascript) => Bindings.webview_eval(m_nativeWebview, javascript);

		public void RunScriptWithReturn(string name, Action<string, string> callback) {
			var callbackInstance = new CallBackFunction((id, req, _) => callback(id, req));
			m_callbacks.Add(callbackInstance);

			Bindings.webview_bind(m_nativeWebview, name, callbackInstance, nint.Zero);
		}

		/// <summary>
		/// Posts a function to be executed on the main thread of the webview.
		/// </summary>
		/// <param name="dispatchFunc">The function to call on the main thread</param>
		public void Dispatch(Action dispatchFunc) {
			DispatchFunction dispatchFuncInstance = null!;
			dispatchFuncInstance = new DispatchFunction((_, __) => {
				lock (m_dispatchFunctions) {
					m_dispatchFunctions.Remove(dispatchFuncInstance);
				}
				dispatchFunc();
			});

			lock (m_dispatchFunctions) {
				m_dispatchFunctions.Add(dispatchFuncInstance);
			}

			Bindings.webview_dispatch(m_nativeWebview, dispatchFuncInstance, nint.Zero);
		}

		public nint GetWindow() => Bindings.webview_get_window(m_nativeWebview);

		public void Terminate() {
			Bindings.webview_terminate(m_nativeWebview);
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			if (!m_disposed) {
				Bindings.webview_terminate(m_nativeWebview);
				Bindings.webview_destroy(m_nativeWebview);
				m_callbacks.Clear();

				lock (m_dispatchFunctions) {
					m_dispatchFunctions.Clear();
				}

				m_disposed = true;
			}
		}

		private void InterceptExternalLinks() {
			// Bind a native method as javascript
			// This method opens the url parameter in the system browser
			RunScriptWithReturn("openExternalLink", (id, req) => {
				string url = ExtractUrlArgument(req);

				ProcessStartInfo psi = new ProcessStartInfo {
					FileName = url,
					UseShellExecute = true
				};
				Process.Start(psi);

				Return(id, RPCResult.Success, "{}");
			});

			// On Init of the webview we inject some javascript
			// This javascript intercepts all click events and checks,
			// if the intercepted click is an external link.
			// In case of an external link the registered native method is called.
			InjectScriptBeforeLoadPages(@"
                function interceptClickEvent(e) {
                    var href = '';
                    var target = e.target || e.srcElement;
                    if (target.tagName === 'A') {
                        href = target.getAttribute('href');
                    }
                    else if(target.tagName === 'IMG') {
                        href = target.parentElement.getAttribute('href');
                    }
                    if(href.startsWith('http') 
                        && !href.startsWith('http://localhost')
                        && !href.startsWith('http://127.0.0.1')
                    ) {
                        openExternalLink(href);
                        e.preventDefault();
                    }
                }

                if (document.addEventListener) {
                    document.addEventListener('click', interceptClickEvent);
                } else if (document.attachEvent) {
                    document.attachEvent('onclick', interceptClickEvent);
                }
            ");
		}

		private static string ExtractUrlArgument(string jsonArray) {
			if (string.IsNullOrWhiteSpace(jsonArray))
				return string.Empty;

			jsonArray = jsonArray.Trim();

			// Expected format: ["http..."]
			if (!jsonArray.StartsWith("[\"http") || !jsonArray.Contains("\"]"))
				return string.Empty;

			int startIndex = 2; // After ["
			int endIndex = jsonArray.IndexOf("\"]", startIndex);

			if (endIndex == -1)
				return string.Empty;

			return jsonArray.Substring(startIndex, endIndex - startIndex);
		}

	}

}
