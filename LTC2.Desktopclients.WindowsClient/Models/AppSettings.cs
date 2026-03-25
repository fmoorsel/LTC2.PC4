namespace LTC2.Desktopclients.WindowsClient.Models
{
    public class AppSettings
    {
        private string _logFolders;
        private string _webviewRoot;
        private string _tilesOKFile;

        private string _multiSportFolder;

        public string Name { get; set; }

        public string WebApp { get; set; }

        public string WebAppParameters { get; set; }

        public bool WebAppNoWindow { get; set; }

        public bool WebAppWindowMinimized { get; set; }

        public string CalculatorApp { get; set; }

        public string CalculatorAppParameters { get; set; }

        public bool CalculatorNoWindow { get; set; }

        public bool CalculatorWindowMinimized { get; set; }

        public string ProfileApp { get; set; }

        public string StartPage { get; set; }

        public string MonitoredComponent { get; set; }

        public string EnableRefreshFor { get; set; }

        public int PingDeltaInSeconds { get; set; }

        public bool DisablePasswordSave { get; set; }

        public string StravaRouteBuilderEntryPoint { get; set; }

        public string StravaRouteBuilderPrefix { get; set; }

        public string StravaSitePrefix { get; set; }

        public string RideWithGpsRouteBuilderEntryPoint { get; set; }

        public string RideWithGpsRouteBuilderPrefix { get; set; }

        public string RideWithGpsSitePrefix { get; set; }

        public string RideWithGpsRouteBuilderPrefixPostfix { get; set; }

        public List<string> SkipCookiesWhileDeleting { get; set; }

        public string TilesOKFile
        {
            get
            {
                return _tilesOKFile == null ? null : Environment.ExpandEnvironmentVariables(_tilesOKFile);
            }

            set
            {
                _tilesOKFile = value;
            }
        }

        public string CustomInstallAction { get; set; }

        public string CustomInstallActionParameters { get; set; }


        public string WebviewRoot
        {
            get
            {
                return _webviewRoot == null ? null : Environment.ExpandEnvironmentVariables(_webviewRoot);
            }
            set
            {
                _webviewRoot = value;
            }
        }

        public string LogFolders
        {
            get
            {
                return _logFolders == null ? null : Environment.ExpandEnvironmentVariables(_logFolders);
            }
            set
            {
                _logFolders = value;
            }
        }

        public string MultiSportFolder
        {
            get
            {
                return _multiSportFolder == null ? null : Environment.ExpandEnvironmentVariables(_multiSportFolder);
            }
            set
            {
                _multiSportFolder = value;
            }
        }

    }
}
