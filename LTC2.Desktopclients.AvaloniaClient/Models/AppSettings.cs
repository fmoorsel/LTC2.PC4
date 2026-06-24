using System;

namespace LTC2.Desktopclients.AvaloniaClient.Models
{
    public class AppSettings
    {
        private string _logFolders;
        private string _logFolder;
        private string _webviewRoot;
        private string _webApp;
        private string _calculatorApp;
        private string _profileApp;
        private string _multiSportFolder;

        public string Name { get; set; }

        public string WebApp
        {
            get => _webApp == null ? null : Environment.ExpandEnvironmentVariables(_webApp);
            set => _webApp = value;
        }

        public string WebAppParameters { get; set; }
        public bool WebAppNoWindow { get; set; }
        public bool WebAppWindowMinimized { get; set; }

        public string CalculatorApp
        {
            get => _calculatorApp == null ? null : Environment.ExpandEnvironmentVariables(_calculatorApp);
            set => _calculatorApp = value;
        }

        public string CalculatorAppParameters { get; set; }
        public bool CalculatorNoWindow { get; set; }
        public bool CalculatorWindowMinimized { get; set; }

        public string ProfileApp
        {
            get => _profileApp == null ? null : Environment.ExpandEnvironmentVariables(_profileApp);
            set => _profileApp = value;
        }

        public string StartPage { get; set; }
        public string MonitoredComponent { get; set; }
        public string EnableRefreshFor { get; set; }
        public int PingDeltaInSeconds { get; set; }
        public bool DisablePasswordSave { get; set; }

        public string LogFolder
        {
            get => _logFolder == null ? null : Environment.ExpandEnvironmentVariables(_logFolder);
            set => _logFolder = value;
        }

        public string LogFolders
        {
            get => _logFolders == null ? null : Environment.ExpandEnvironmentVariables(_logFolders);
            set => _logFolders = value;
        }

        public string WebviewRoot
        {
            get => _webviewRoot == null ? null : Environment.ExpandEnvironmentVariables(_webviewRoot);
            set => _webviewRoot = value;
        }

        public string MultiSportFolder
        {
            get => _multiSportFolder == null ? null : Environment.ExpandEnvironmentVariables(_multiSportFolder);
            set => _multiSportFolder = value;
        }

        public string StravaRouteBuilderEntryPoint { get; set; }
        public string StravaRouteBuilderRegex { get; set; }
        public string StravaSitePrefix { get; set; }
        public string RideWithGpsRouteBuilderEntryPoint { get; set; }
        public string RideWithGpsRouteBuilderRegex { get; set; }
        public string RideWithGpsSitePrefix { get; set; }
    }
}
