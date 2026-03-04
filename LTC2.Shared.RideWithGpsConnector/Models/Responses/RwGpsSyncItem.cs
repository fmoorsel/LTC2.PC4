using System;

namespace LTC2.Shared.RideWithGpsConnector.Models.Responses
{
    public class RwGpsSyncItem
    {
        public string Item_type { get; set; }
        public long Item_id { get; set; }
        public long Item_user_id { get; set; }
        public string Item_url { get; set; }
        public string Action { get; set; }
        public DateTime Datetime { get; set; }
    }
}
