namespace LTC2.Shared.Models.Interprocess
{
    public class StatusMessage
    {
        public const int PING_DELTA_STARTUP_SLACK = 12;

        public const string ORG_CALCULATOR = "calculator";

        public const string ORG_WEBAPP = "webapp";


        public const string STATUS_PING = "ping";


        public const string STATUS_FATAL = "fatal";


        public const string STATUS_RESULT = "result";


        public const string STATUS_STARTUPDATE = "startupdate";


        public const string STATUS_ENDUPDATE = "endupdate";


        public const string STATUS_CHECK = "check";


        public const string STATUS_LIMIT = "limit";


        public const string STATUS_WAIT = "wait";


        public const string STATUS_START = "start";

        public string Status { get; set; }

        public string Origin { get; set; }

        public string Message { get; set; }
    }
}
