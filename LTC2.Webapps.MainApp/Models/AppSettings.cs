using System;

namespace LTC2.Webapps.MainApp.Models
{
    public class AppSettings
    {
        private string _tilesFolder;
        public string Name { get; set; }

        public string AllowedOrigins { get; set; }

        public bool ForceDetailed { get; set; }

        public string TilesFolder
        {
            get
            {
                return _tilesFolder == null ? null : Environment.ExpandEnvironmentVariables(_tilesFolder);
            }

            set
            {
                _tilesFolder = value;
            }
        }

        public string DefaultListenUrls { get; set; }

        public bool IsStandAlone { get; set; }
    }
}
