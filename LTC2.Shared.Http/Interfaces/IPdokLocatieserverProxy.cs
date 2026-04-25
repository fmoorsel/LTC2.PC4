using LTC2.Shared.Http.Models.Responses;
using System.Threading.Tasks;

namespace LTC2.Shared.Http.Interfaces
{
    public interface IPdokLocatieserverProxy
    {
        Task<PdokFreeResponse> FreeAsync(string query, int rows = 1, string filterQuery = null);
    }
}
