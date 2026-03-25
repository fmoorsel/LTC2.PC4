using System.Collections.Generic;

namespace LTC2.Shared.Models.Requests
{
    public class CheckLineStringsRequest
    {
        public List<List<double[]>> Lines { get; set; } = new List<List<double[]>>();
    }
}
