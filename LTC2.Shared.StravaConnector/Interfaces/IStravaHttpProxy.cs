using LTC2.Shared.StravaConnector.Models.Requests;
using LTC2.Shared.StravaConnector.Models.Responses;
using System.Threading.Tasks;

namespace LTC2.Shared.StravaConnector.Interfaces
{
    public interface IStravaHttpProxy
    {
        public Task<AuthorizeResponse> GetToken(AuthorizeRequest request);

        public Task<GetActivitiesResponse> GetActivities(GetActivitiesRequest request, string accessToken);

        public Task<GetActivityCoordinateStreamResponse> GetActivityCoordinateStream(GetActivityCoordinateStreamRequest request, string accessToken);
    }
}
