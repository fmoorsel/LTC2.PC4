namespace LTC2.DesktopClients.ArchiveImporter.Models
{
    public class AppSettings
    {

        private string _tempFolder;

        public string Name { get; set; }

        public List<string> ActivityTypes { get; set; }

        public List<string> SupportedFormats { get; set; }

        public double MaxDistance { get; set; } = 0.1;

        public string TempFolder
        {
            get
            {
                return _tempFolder == null ? null : Environment.ExpandEnvironmentVariables(_tempFolder);
            }
            set
            {
                _tempFolder = value;
            }
        }
    }
}
