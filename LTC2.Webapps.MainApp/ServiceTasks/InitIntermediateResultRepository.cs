using LTC2.Shared.Repositories.Interfaces;
using LTC2.Shared.Utils.Bootstrap.Interfaces;
using System.Threading.Tasks;

namespace LTC2.Services.Calculator.ServiceTasks
{
    public class InitIntermediateResultRepository : IServiceTask
    {

        private readonly IIntermediateResultsRepository _intermediateResulstRepository;

        public InitIntermediateResultRepository(IIntermediateResultsRepository intermediateResultsRepository)
        {
            _intermediateResulstRepository = intermediateResultsRepository;
        }

        public Task ExecuteAsync()
        {
            _intermediateResulstRepository.Open();

            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            _intermediateResulstRepository.Close();

            return Task.CompletedTask;
        }
    }
}
