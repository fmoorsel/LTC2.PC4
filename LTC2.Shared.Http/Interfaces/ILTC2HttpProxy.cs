using System.Collections.Generic;
using System.Threading.Tasks;

namespace LTC2.Shared.Http.Interfaces
{
    public interface ILTC2HttpProxy
    {
        public Task Update(string token, bool refresh, bool byPassCache, bool isRestore, bool isClear, string source = null);

        public Task UpdateMulti(string token, List<int> types, List<string> rwGpsTypes, bool refresh, bool byPassCache, bool isRestore, bool isClear, string source = null);

        public Task<bool> HasIntermediateResult(string accessToken, bool multi);
    }
}
