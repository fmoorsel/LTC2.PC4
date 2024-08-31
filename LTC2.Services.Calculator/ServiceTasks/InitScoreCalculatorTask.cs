using LTC2.Services.Calculator.Interfaces;
using LTC2.Shared.Utils.Bootstrap.Interfaces;
using System.Threading.Tasks;

namespace LTC2.Services.Calculator.ServiceTasks
{
    public class InitScoreCalculatorTask : IServiceTask
    {

        private readonly IScoreCalculator _scoreCalculator;

        public InitScoreCalculatorTask(IScoreCalculator scoreCalculator)
        {
            _scoreCalculator = scoreCalculator;
        }

        public Task ExecuteAsync()
        {
            _scoreCalculator.Init();

            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            return Task.CompletedTask;
        }
    }
}
