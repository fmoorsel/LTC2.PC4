using System;

namespace LTC2.Shared.Models.Settings
{
    public class GenericSettings
    {
        private string _sessionsFolder;
        private string _secretsFolder;
        private string _cacheFolder;
        private string _intermediateResultsFolder;
        private string _databaseConnectionSqliteString;

        public string Id { get; set; }

        public string Name { get; set; }

        public string ResourceFolder { get; set; }

        public string PlacesFile { get; set; }

        public string NeighboursFile { get; set; }

        public bool ForceReplaceMap { get; set; }

        public string SessionsFolder
        {
            get
            {
                return _sessionsFolder == null ? null : Environment.ExpandEnvironmentVariables(_sessionsFolder);
            }

            set
            {
                _sessionsFolder = value;
            }
        }

        public string SecretsFolder
        {
            get
            {
                return _secretsFolder == null ? null : Environment.ExpandEnvironmentVariables(_secretsFolder);
            }

            set
            {
                _secretsFolder = value;
            }
        }

        public string CacheFolder
        {
            get
            {
                return _cacheFolder == null ? null : Environment.ExpandEnvironmentVariables(_cacheFolder);
            }

            set
            {
                _cacheFolder = value;
            }
        }

        public string IntermediateResultsFolder
        {
            get
            {
                return _intermediateResultsFolder == null ? null : Environment.ExpandEnvironmentVariables(_intermediateResultsFolder);
            }

            set
            {
                _intermediateResultsFolder = value;
            }
        }

        public int SessionIsExpiringAfterSeconds { get; set; }

        public string DatabaseConnectionString { get; set; }

        public string DatabaseConnectionSqliteString
        {
            get
            {
                return _databaseConnectionSqliteString == null ? null : Environment.ExpandEnvironmentVariables(_databaseConnectionSqliteString);
            }

            set
            {
                _databaseConnectionSqliteString = value;
            }
        }
    }
}
