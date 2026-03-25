using LTC2.Shared.Models.Requests;
using LTC2.Shared.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LTC2.Shared.Http.Interfaces
{
    public interface ILTC2HttpProxy
    {
        public Task Update(string token, bool refresh, bool byPassCache, bool isRestore, bool isClear, string source = null);

        public Task UpdateMulti(string token, List<int> types, List<string> rwGpsTypes, bool refresh, bool byPassCache, bool isRestore, bool isClear, string source = null);

        public Task<bool> HasIntermediateResult(string accessToken, bool multi);

        public Task<GetProfileResponse> GetProfile(string accessToken, bool multi);

        public Task<List<string>> CheckLineStrings(string accessToken, CheckLineStringsRequest request);
    }
}
