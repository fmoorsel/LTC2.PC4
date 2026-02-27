using LTC2.Shared.Common.Interfaces;
using LTC2.Shared.StravaConnector.Models.Requests;
using LTC2.Shared.StravaConnector.Models.Responses;
using System.Threading.Tasks;

namespace LTC2.Shared.StravaConnector.Interfaces
{
    public interface IStravaConnector : IConnector
    {
        public Task<GetRoutesResponse> GetRoutes(GetRoutesRequest request);

        public Task<GetRouteDetailsAsGpxReponse> GetRouteDetailsAsGpx(GetRouteDetailsAsGpxRequest request);
    }
}
