using LTC2.Shared.Models.Domain;
using System.Collections.Generic;

namespace LTC2.Shared.Models.Responses
{
    public class GetRoutesResponse : ConnectorResponse
    {
        public List<SourceRoute> Routes { get; set; } = [];
    }
}
