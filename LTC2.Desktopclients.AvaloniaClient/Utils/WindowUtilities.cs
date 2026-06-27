using Avalonia.Controls;
using System;

namespace LTC2.Desktopclients.AvaloniaClient.Utils
{
    public static class WindowUtilities
    {
#if WINDOWS
        [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr hwnd, int nIndex);
        [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern int SetWindowLong(IntPtr hwnd, int nIndex, int value);
        private const int GWL_STYLE = -16;
        private const int WS_MINIMIZEBOX = 0x00020000;
#endif

        public static void RemoveMinimizeButton(Window window)
        {
#if WINDOWS
            var hwnd = window.TryGetPlatformHandle()?.Handle;
            if (hwnd.HasValue)
                SetWindowLong(hwnd.Value, GWL_STYLE, GetWindowLong(hwnd.Value, GWL_STYLE) & ~WS_MINIMIZEBOX);
#else
            // intentionally empty due to cross-platform concerns
#endif
        }
    }
}
