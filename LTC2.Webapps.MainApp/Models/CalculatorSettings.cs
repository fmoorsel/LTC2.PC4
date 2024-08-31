using System;

namespace LTC2.Webapps.MainApp.Models
{
    public class CalculatorSettings
    {
        private string _brokerConnection;

        public string BrokerConnection
        {
            get
            {
                return _brokerConnection == null ? null : Environment.ExpandEnvironmentVariables(_brokerConnection);
            }

            set
            {
                _brokerConnection = value;
            }
        }
    }
}
