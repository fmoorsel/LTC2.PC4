namespace LTC2.Desktopclients.ProfileManager.Models
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

        public string SecretsFolder { get; set; }

        public bool DisablePasswordSave { get; set; }

        public int PingDeltaInSeconds { get; set; }


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
    }
}
