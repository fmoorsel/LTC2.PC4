using LTC2.Shared.Repositories.Interfaces;
using LTC2.Shared.Utils.Bootstrap.Interfaces;
using System.Threading.Tasks;

namespace LTC2.Webapps.MainApp.ServiceTasks
{
    public class InitScoreRepositoryTask : IServiceTask
    {
        private readonly IScoresRepository _scoresRepository;

        public InitScoreRepositoryTask(IScoresRepository scoresRepository)
        {
            _scoresRepository = scoresRepository;
        }

        public Task ExecuteAsync()
        {
            _scoresRepository.Open();

            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            return Task.CompletedTask;
        }
    }
}
