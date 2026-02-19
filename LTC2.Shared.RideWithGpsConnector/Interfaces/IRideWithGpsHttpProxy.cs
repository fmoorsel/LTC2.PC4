using LTC2.Shared.RideWithGpsConnector.Models;
using LTC2.Shared.RideWithGpsConnector.Models.Requests;
using LTC2.Shared.RideWithGpsConnector.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LTC2.Shared.RideWithGpsConnector.Interfaces
{
    public interface IRideWithGpsHttpProxy
    {
        Task<AuthorizeResponse> GetToken(AuthorizeRequest request);
        Task<CurrentUserResponse> GetCurrentUser(string accessToken);
        Task<List<RwGpsSyncItem>> GetActivities(GetActivitiesRequest request, string accessToken);
        Task<RideWithGpsTrip> GetTrip(long id, string accessToken);
    }
}
