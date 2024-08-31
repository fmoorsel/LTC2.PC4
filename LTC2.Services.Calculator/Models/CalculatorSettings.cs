using System;
using System.Collections.Generic;

namespace LTC2.Services.Calculator.Models
{
    public class CalculatorSettings
    {
        private string _brokerConnection;

        public string MapName { get; set; }

        public string ActivitiesName { get; set; }

        public int MaxPlacesCount { get; set; }

        public List<string> ActivityTypes { get; set; }

        public List<string> WhiteListedActivities { get; set; }

        public List<string> BlackListedActivities { get; set; }

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


        public int IntermediateResultAfterCount { get; set; }
    }
}
