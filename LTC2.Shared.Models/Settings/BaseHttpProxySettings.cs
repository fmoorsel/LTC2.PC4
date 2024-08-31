namespace LTC2.Shared.Models.Settings
{
    public class BaseHttpProxySettings
    {
        public string Url { get; set; }

        public int TimeOut { get; set; }

        public bool Pooled { get; set; }
    }
}
