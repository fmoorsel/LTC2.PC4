using LTC2.Shared.Models.Domain;
using System.Collections.Generic;

namespace LTC2.Shared.Common.Models
{
    public class GetRoutesResponse : ConnectorResponse
    {
        public List<SourceRoute> Routes { get; set; } = [];
    }
}
