using LTC2.Services.Calculator.Services;
using LTC2.Shared.Models.Interprocess;
using LTC2.Shared.Utils.Bootstrap.Interfaces;
using LTC2.Shared.Utils.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace LTC2.Services.Calculator.ServiceTasks
{
    public class InitStatusPublisherTask : IServiceTask
    {
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;

        private Task _executionTask;

        private StreamString _streamString;

        private readonly StatusNotifier _statusNotifier;
        private readonly ILogger<InitStatusPublisherTask> _logger;

        public InitStatusPublisherTask(StatusNotifier statusNotifier, ILogger<InitStatusPublisherTask> logger)
        {
            _statusNotifier = statusNotifier;
            _logger = logger;
        }

        public Task ExecuteAsync()
        {
            var pipeName = GetPipeName();

            if (pipeName != null)
            {
                _executionTask = Task.Factory.StartNew(() =>
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                    _cancellationToken = _cancellationTokenSource.Token;

                    ProccesorLoop(pipeName, _cancellationToken);

                }, TaskCreationOptions.LongRunning);
            }

            return Task.CompletedTask;
        }

        private void ProccesorLoop(string pipeName, CancellationToken cancellationToken)
        {
            var proceed = !cancellationToken.IsCancellationRequested;

            using (var pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.Out))
            {
                pipeClient.Connect();

                _statusNotifier.Enabled = true;
                _streamString = new StreamString(pipeClient);

                var lastPingSend = DateTime.UtcNow.AddMilliseconds(-2000);

                while (proceed)
                {
                    try
                    {
                        proceed = !cancellationToken.IsCancellationRequested;


                        if (proceed)
                        {
                            var status = _statusNotifier.GetNotification();

                            proceed = !cancellationToken.IsCancellationRequested;

                            if (proceed && status != null)
                            {
                                var messageContent = JsonConvert.SerializeObject(status);

                                _streamString.WriteString(messageContent);
                            }

                            var timespanLastPingSend = DateTime.UtcNow - lastPingSend;

                            if (proceed && timespanLastPingSend.TotalMilliseconds >= 2000)
                            {
                                lastPingSend = DateTime.UtcNow;

                                var statusMessage = new StatusMessage()
                                {
                                    Status = StatusMessage.STATUS_PING,
                                    Origin = StatusMessage.ORG_CALCULATOR,
                                    Message = DateTime.UtcNow.ToString()
                                };

                                var messageContent = JsonConvert.SerializeObject(statusMessage);

                                _streamString.WriteString(messageContent);
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        proceed = false;
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, e.Message);

                        proceed = false;
                    }
                }

                _streamString = null;
            }
        }

        public Task StopAsync()
        {
            if (_executionTask != null && _cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();

                _executionTask.Wait();
            }

            return Task.CompletedTask;
        }

        private string GetPipeName()
        {
            var arguments = Environment.GetCommandLineArgs();

            foreach (var parameter in arguments)
            {
                var pipeParToken = "pipe:";
                if (parameter.ToLower().StartsWith(pipeParToken))
                {
                    return parameter.Substring(pipeParToken.Length);
                }
            }

            return null;
        }
    }
}
