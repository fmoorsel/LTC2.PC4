using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LTC2.Shared.Utils.Generic
{
    public class ProcessUtils
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        private const int SW_HIDE = 0;
        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;
        private const int SW_SHOWMAXIMIZED = 3;
        private const int SW_SHOWNOACTIVATE = 4;
        private const int SW_RESTORE = 9;
        private const int SW_SHOWDEFAULT = 10;

        public static void EnsureOnlyOneProcess()
        {
            var proc = Process.GetCurrentProcess().ProcessName;
            var processes = Process.GetProcessesByName(proc);

            if (processes.Length > 1)
            {
                var p = Process.GetCurrentProcess();
                var n = 0;
                var other = 1;

                if (processes[0].Id == p.Id)
                {
                    n = 1;
                    other = 0;
                }

                var hWnd = processes[n].MainWindowHandle;

                if (IsIconic(hWnd))
                {
                    ShowWindowAsync(hWnd, SW_RESTORE);
                }

                SetForegroundWindow(hWnd);

                var hWndOther = processes[other].MainWindowHandle;

                if (IsIconic(hWndOther))
                {
                    ShowWindowAsync(hWndOther, SW_RESTORE);
                }

                SetForegroundWindow(hWndOther);

                Environment.Exit(0);
            }
        }
    }
}
