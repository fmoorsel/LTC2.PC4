using System;

namespace LTC2.Shared.RideWithGpsConnector.Models.Responses
{
    public class RwGpsUser
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime? Created_at { get; set; }
        public DateTime? Updated_at { get; set; }
    }
}
