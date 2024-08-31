using LTC2.Services.Calculator.Models;
using LTC2.Shared.Models.Settings;
using LTC2.Shared.Repositories.Interfaces;
using LTC2.Shared.StravaConnector.Interfaces;
using LTC2.Shared.Utils.Bootstrap.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace LTC2.Services.Calculator.ServiceTasks
{
    public class InitMapRepositoryTask : IServiceTask
    {
        private readonly IMapRepository _mapRepository;
        private readonly ILogger<InitMapRepositoryTask> _logger;
        private readonly CalculatorSettings _calculatorSettings;
        private readonly GenericSettings _genericSettings;
        private readonly IStravaConnector _stravaConnector;

        public InitMapRepositoryTask(
            ILogger<InitMapRepositoryTask> logger,
            IMapRepository mapRepository,
            IStravaConnector stravaConnector,
            CalculatorSettings calculatorSettings,
            GenericSettings genericSettings)
        {
            _mapRepository = mapRepository;
            _logger = logger;
            _calculatorSettings = calculatorSettings;
            _genericSettings = genericSettings;
            _stravaConnector = stravaConnector;
        }

        public Task ExecuteAsync()
        {
            try
            {
                _logger.LogInformation($"Init map repository, using type: {_mapRepository.GetType().Name}");

                _mapRepository.CheckPreparedMap();
                _mapRepository.Open();

                if (_genericSettings.ForceReplaceMap || !_mapRepository.HasMap())
                {
                    _mapRepository.CreateAndPopulateMapIndex(_genericSettings.ForceReplaceMap);
                }

                _logger.LogInformation($"Map repository initialized, using type: {_mapRepository.GetType().Name}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unable to initialize map repository: {e.Message}");

                throw;
            }

            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            _mapRepository.Close();

            return Task.CompletedTask;
        }
    }
}
