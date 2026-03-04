using System.Collections.Generic;

namespace LTC2.Shared.Models.Requests
{
    public class UpdateMultiRequest
    {
        public List<int> Types { get; set; }
        public List<string> RwGpsTypes { get; set; }
    }
}
