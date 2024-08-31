using System.Threading.Tasks;

namespace LTC2.Shared.Utils.Bootstrap.Interfaces
{
    public interface IServiceTask
    {
        Task ExecuteAsync();

        Task StopAsync();
    }
}
