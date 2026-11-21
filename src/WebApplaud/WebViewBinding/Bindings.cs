using System.Runtime.InteropServices;

namespace WebApplaud.WebViewBinding {

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void DispatchFunction(nint webview, nint arguments);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void CallBackFunction([MarshalAs(UnmanagedType.LPUTF8Str)] string id, [MarshalAs(UnmanagedType.LPUTF8Str)] string request, nint argument);

	internal static class Bindings {
		private const string DllFile = "webview";

		[DllImport(DllFile, CallingConvention = CallingConvention.Cdecl)]
		internal extern static nint webview_create(int debug, nint window);

		[DllImport(DllFile, CallingConvention = CallingConvention.Cdecl)]
		internal extern static void webview_destroy(nint webview);

		[DllImport(DllFile, CallingConvention = CallingConvention.Cdecl)]
		internal extern static void webview_run(nint webview);

		[DllImport(DllFile, CallingConvention = CallingConvention.Cdecl)]
		internal extern static void webview_terminate(nint webview);

		[DllImport(DllFile, CallingConvention = CallingConvention.Cdecl)]
		internal extern static void webview_set_title(nint webview, [MarshalAs(UnmanagedType.LPUTF8Str)] string title);

		[DllImport(DllFile, CallingConvention = CallingConvention.Cdecl)]
		internal extern static void webview_set_size(nint webview, int width, int height, WebviewHint hint);

		[DllImport(DllFile, CallingConvention = CallingConvention.Cdecl)]
		internal extern static void webview_navigate(nint webview, [MarshalAs(UnmanagedType.LPUTF8Str)] string url);

		[DllImport(DllFile, CallingConvention = CallingConvention.Cdecl)]
		internal extern static void webview_init(nint webview, [MarshalAs(UnmanagedType.LPUTF8Str)] string js);

		[DllImport(DllFile, CallingConvention = CallingConvention.Cdecl)]
		internal extern static void webview_eval(nint webview, [MarshalAs(UnmanagedType.LPUTF8Str)] string js);

		[DllImport(DllFile, CallingConvention = CallingConvention.Cdecl)]
		internal extern static void webview_dispatch(nint webview, DispatchFunction dispatchFunction, nint args);

		[DllImport(DllFile, CallingConvention = CallingConvention.Cdecl)]
		internal extern static void webview_bind(nint webview, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, CallBackFunction callback, nint arg);

		[DllImport(DllFile, CallingConvention = CallingConvention.Cdecl)]
		internal extern static void webview_return(nint webview, [MarshalAs(UnmanagedType.LPUTF8Str)] string id, RPCResult result, [MarshalAs(UnmanagedType.LPUTF8Str)] string resultJson);

		[DllImport(DllFile, CallingConvention = CallingConvention.Cdecl)]
		internal extern static nint webview_get_window(nint webview);

	}

}
