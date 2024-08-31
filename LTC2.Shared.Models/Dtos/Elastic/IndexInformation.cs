using System;

namespace LTC2.Shared.Models.Dtos.Elastic
{
    public class IndexInformation
    {
        public string Name { get; set; }

        public string IndexName { get; set; }

        public DateTime CreationTime { get; set; }

        public bool IsActive { get; set; }

    }
}
