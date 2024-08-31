using LTC2.Services.Calculator.Interfaces;
using LTC2.Services.Calculator.Models;
using LTC2.Services.Calculator.Services;
using LTC2.Shared.Messaging.Generic;
using LTC2.Shared.Messaging.Implementations.FileBasedBroker;
using LTC2.Shared.Messaging.Interfaces;
using LTC2.Shared.Models.Domain;
using LTC2.Shared.Models.Interprocess;
using LTC2.Shared.Utils.Bootstrap.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace LTC2.Services.Calculator.ServiceTasks
{
    public class InitFolderScannerTask : IServiceTask
    {
        private readonly CalculatorSettings _settings;
        private readonly ILogger<InitFolderScannerTask> _logger;
        private readonly IBrokerFactory<Message> _brokerFactory;
        private readonly IScoreCalculator _scoreCalculator;

        private IBrokerConnection _connection;
        private IConsumer _consumer;
        private readonly StatusNotifier _statusNotifier;

        public InitFolderScannerTask(
                CalculatorSettings settings,
                IScoreCalculator scoreCalculator,
                StatusNotifier statusNotifier,
                ILogger<InitFolderScannerTask> logger,
                IBrokerFactory<Message> brokerFactory
            )
        {
            _settings = settings;
            _logger = logger;
            _brokerFactory = brokerFactory;
            _scoreCalculator = scoreCalculator;
            _statusNotifier = statusNotifier;

        }
        public Task ExecuteAsync()
        {
            _logger.LogInformation("Setting up connection with message broker for calculation job receival");

            var broker = _brokerFactory.CreateBroker();

            _connection = broker.Connect(_settings.BrokerConnection);

            var target = new Target()
            {
                Name = "INPUT"
            };

            _consumer = _connection.AddConsumer(target, OnMessage);

            return Task.CompletedTask;
        }


        private async Task OnMessage(IMessage message)
        {
            try
            {
                NotifyUpdate(true);

                if (message.Type == MessageType.Text)
                {
                    var messageContent = message.Payload as string;

                    var calculationJob = JsonConvert.DeserializeObject<CalculationJob>(messageContent);

                    await _scoreCalculator.Calculate(calculationJob);
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"Unable to process message with payload {message.Payload}");
            }
            finally
            {
                message.Acknowlegde();

                NotifyUpdate(false);
            }
        }

        private void NotifyUpdate(bool start)
        {
            if (start)
            {
                _logger.LogDebug($"Start update {DateTime.UtcNow}.");

                _statusNotifier.SetNotification(StatusMessage.STATUS_STARTUPDATE, $"Update started {DateTime.UtcNow}");
            }
            else
            {
                _logger.LogDebug($"End update {DateTime.UtcNow}.");

                _statusNotifier.SetNotification(StatusMessage.STATUS_ENDUPDATE, $"Update ended {DateTime.UtcNow}");
            }
        }

        public Task StopAsync()
        {
            _consumer.Close();
            _connection.Close();

            return Task.CompletedTask;
        }
    }
}
