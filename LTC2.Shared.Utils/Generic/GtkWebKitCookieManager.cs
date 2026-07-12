using System;

#if LINUX
using System.IO;
using System.Runtime.InteropServices;
#endif

namespace LTC2.Shared.Utils.Generic
{
    public static class GtkWebKitCookieManager
    {
#if LINUX
        private const int WebKitCookiePersistentStorageSqlite = 1;

        static GtkWebKitCookieManager()
        {
            var candidates = new[]
            {
                "libwebkit2gtk-4.1.so.0",
                "libwebkit2gtk-4.1.so",
                "libwebkit2gtk-4.0.so.37",
                "libwebkit2gtk-4.0.so"
            };

            NativeLibrary.SetDllImportResolver(typeof(GtkWebKitCookieManager).Assembly, (name, assembly, searchPath) =>
            {
                if (name != "libwebkit2gtk")
                {
                    return IntPtr.Zero;
                }

                foreach (var candidate in candidates)
                {
                    if (NativeLibrary.TryLoad(candidate, assembly, searchPath, out var handle))
                    {
                        return handle;
                    }
                }

                return IntPtr.Zero;
            });
        }

        [DllImport("libwebkit2gtk")]
        private static extern IntPtr webkit_web_view_get_context(IntPtr webView);

        [DllImport("libwebkit2gtk")]
        private static extern IntPtr webkit_web_context_get_cookie_manager(IntPtr context);

        [DllImport("libwebkit2gtk")]
        private static extern void webkit_cookie_manager_set_persistent_storage(IntPtr cookieManager, string filename, int storage);

        // WebKitGTK only persists cookies to disk if a cookie manager is explicitly pointed at a
        // storage file - otherwise it keeps them in memory for the lifetime of the process.
        public static void EnablePersistentStorage(IntPtr webKitWebViewHandle, string cookieStoreFilePath)
        {
            if (webKitWebViewHandle == IntPtr.Zero || string.IsNullOrEmpty(cookieStoreFilePath))
            {
                return;
            }

            var directory = Path.GetDirectoryName(cookieStoreFilePath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var context = webkit_web_view_get_context(webKitWebViewHandle);

            if (context == IntPtr.Zero)
            {
                return;
            }

            var cookieManager = webkit_web_context_get_cookie_manager(context);

            if (cookieManager == IntPtr.Zero)
            {
                return;
            }

            webkit_cookie_manager_set_persistent_storage(cookieManager, cookieStoreFilePath, WebKitCookiePersistentStorageSqlite);
        }
#else
        public static void EnablePersistentStorage(IntPtr webKitWebViewHandle, string cookieStoreFilePath)
        {
            // intentionally empty outside Linux
        }
#endif
    }
}
