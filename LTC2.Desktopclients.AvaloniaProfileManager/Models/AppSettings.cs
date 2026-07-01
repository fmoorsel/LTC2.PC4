using System;

namespace LTC2.Desktopclients.AvaloniaProfileManager.Models
{
    public class AppSettings
    {
        private string _webviewRoot;
        private string _webApp;
        private string _libraryPath;

        public string Name { get; set; }
        public string WebAppParameters { get; set; }
        public bool WebAppNoWindow { get; set; }
        public bool WebAppWindowMinimized { get; set; }
        public string StartPage { get; set; }
        public string MonitoredComponent { get; set; }
        public bool DisablePasswordSave { get; set; }
        public int PingDeltaInSeconds { get; set; }

        public string WebApp
        {
            get => _webApp == null ? null : Environment.ExpandEnvironmentVariables(_webApp);
            set => _webApp = value;
        }

        public string LibraryPath
        {
            get => _libraryPath == null ? null : Environment.ExpandEnvironmentVariables(_libraryPath);
            set => _libraryPath = value;
        }

        public string WebviewRoot
        {
            get => _webviewRoot == null ? null : Environment.ExpandEnvironmentVariables(_webviewRoot);
            set => _webviewRoot = value;
        }
    }
}
