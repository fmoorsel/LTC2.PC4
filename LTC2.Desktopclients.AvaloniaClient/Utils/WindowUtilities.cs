using Avalonia.Controls;
using System;
using System.Runtime.InteropServices;

namespace LTC2.Desktopclients.AvaloniaClient.Utils
{
    public static class WindowUtilities
    {
#if WINDOWS
        [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr hwnd, int nIndex);
        [DllImport("user32.dll")] private static extern int SetWindowLong(IntPtr hwnd, int nIndex, int value);
        private const int GWL_STYLE = -16;
        private const int WS_MINIMIZEBOX = 0x00020000;
#elif LINUX
        [DllImport("libX11.so.6")] private static extern IntPtr XOpenDisplay(string display);
        [DllImport("libX11.so.6")] private static extern int XCloseDisplay(IntPtr display);
        [DllImport("libX11.so.6")] private static extern IntPtr XInternAtom(IntPtr display, string atomName, bool onlyIfExists);
        [DllImport("libX11.so.6")] private static extern int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, int mode, ref MotifWmHints data, int nelements);

        [StructLayout(LayoutKind.Sequential)]
        private struct MotifWmHints
        {
            public ulong Flags;
            public ulong Functions;
            public ulong Decorations;
            public long  InputMode;
            public ulong Status;
        }

        private const int    PropModeReplace     = 0;
        private const ulong  MwmHintsFunctions   = 1UL;
        private const ulong  MwmHintsDecorations = 2UL;
        private const ulong  MwmFuncResize        = 2UL;
        private const ulong  MwmFuncMove          = 4UL;
        private const ulong  MwmFuncMaximize      = 16UL;
        private const ulong  MwmFuncClose         = 32UL;
        private const ulong  MwmDecorBorder       = 2UL;
        private const ulong  MwmDecorTitle        = 8UL;
        private const ulong  MwmDecorMenu         = 16UL;
        private const ulong  MwmDecorMaximize     = 64UL;
#endif

        public static void RemoveMinimizeButton(Window window)
        {
#if WINDOWS
            var hwnd = window.TryGetPlatformHandle()?.Handle;
            if (hwnd.HasValue)
                SetWindowLong(hwnd.Value, GWL_STYLE, GetWindowLong(hwnd.Value, GWL_STYLE) & ~WS_MINIMIZEBOX);
#elif LINUX
            var handle = window.TryGetPlatformHandle();
            if (handle?.HandleDescriptor != "XID") return;  // not X11 (e.g. native Wayland)

            var display = XOpenDisplay(null);
            if (display == IntPtr.Zero) return;

            try
            {
                var motifAtom = XInternAtom(display, "_MOTIF_WM_HINTS", false);
                var hints = new MotifWmHints
                {
                    Flags       = MwmHintsFunctions | MwmHintsDecorations,
                    Functions   = MwmFuncResize | MwmFuncMove | MwmFuncMaximize | MwmFuncClose,
                    Decorations = MwmDecorBorder | MwmDecorTitle | MwmDecorMenu | MwmDecorMaximize,
                };
                XChangeProperty(display, handle.Handle, motifAtom, motifAtom, 32, PropModeReplace, ref hints, 5);
            }
            finally
            {
                XCloseDisplay(display);
            }
#endif
        }
    }
}
