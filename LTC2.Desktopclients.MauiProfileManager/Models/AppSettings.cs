using System;

namespace LTC2.Desktopclients.MauiProfileManager.Models
{
    public class AppSettings
    {
        private string _webviewRoot;

        public string Name { get; set; }
        public string WebApp { get; set; }
        public string WebAppParameters { get; set; }
        public bool WebAppNoWindow { get; set; }
        public bool WebAppWindowMinimized { get; set; }
        public string StartPage { get; set; }
        public string MonitoredComponent { get; set; }
        public bool DisablePasswordSave { get; set; }
        public int PingDeltaInSeconds { get; set; }
        public string LibraryPath { get; set; }

        public string WebviewRoot
        {
            get => _webviewRoot == null ? null : Environment.ExpandEnvironmentVariables(_webviewRoot);
            set => _webviewRoot = value;
        }
    }
}
