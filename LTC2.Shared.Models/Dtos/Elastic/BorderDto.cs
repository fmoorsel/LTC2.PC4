using System.Collections.Generic;

namespace LTC2.Shared.Models.Dtos.Elastic
{
    public class BorderDto
    {
        public string Type { get; set; } = "Polygon";

        public List<List<List<double>>> Coordinates { get; set; } = new List<List<List<double>>>();
    }
}
