using LTC2.Shared.Common.Interfaces;
using LTC2.Shared.StravaConnector.Models.Requests;
using LTC2.Shared.StravaConnector.Models.Responses;

namespace LTC2.Shared.StravaConnector.Interfaces
{
    public interface IStravaConnector : IConnector<GetActivitiesRequest, GetActivitiesResponse,
        GetActivityCoordinateStreamRequest, GetActivityCoordinateStreamResponse,
        GetRoutesRequest, GetRoutesResponse,
        GetRouteDetailsAsGpxRequest, GetRouteDetailsAsGpxReponse>
    {
    }
}
