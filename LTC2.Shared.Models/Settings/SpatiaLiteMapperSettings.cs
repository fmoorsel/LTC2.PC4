using System;

namespace LTC2.Shared.Models.Settings
{
    public class SpatiaLiteMapperSettings
    {
        private string _connectionString;

        public string ConnectionString
        {
            get
            {
                return _connectionString == null ? null : Environment.ExpandEnvironmentVariables(_connectionString);
            }

            set
            {
                _connectionString = value;
            }
        }

        public string PreparedMap { get; set; }

    }
}
