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


        public const string STATUS_PROFILESELECTED = "profileselected";

        public string Status { get; set; }

        public string Origin { get; set; }

        public string Message { get; set; }

        // Environment.TickCount64 (ms since boot) at the moment of a PING. Used for keep-alive freshness
        // comparisons instead of Message, since wall-clock time (DateTime.UtcNow) can jump due to NTP/VM
        // clock corrections and falsely trip the keep-alive timeout. Message is kept for display purposes.
        public long Ticks { get; set; }
    }
}
