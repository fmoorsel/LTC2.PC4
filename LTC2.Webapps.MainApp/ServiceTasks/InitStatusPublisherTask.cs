using LTC2.Shared.Models.Interprocess;
using LTC2.Shared.Utils.Bootstrap.Interfaces;
using LTC2.Shared.Utils.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace LTC2.Webapps.MainApp.ServiceTasks
{
    public class InitStatusPublisherTask : IServiceTask
    {
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;

        private Task _executionTask;

        private readonly ILogger<InitStatusPublisherTask> _logger;

        public InitStatusPublisherTask(ILogger<InitStatusPublisherTask> logger)
        {
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

                var stream = new StreamString(pipeClient);
                var firstPingDone = false;

                while (proceed)
                {
                    try
                    {
                        var statusMessage = new StatusMessage()
                        {
                            Status = StatusMessage.STATUS_PING,
                            Origin = StatusMessage.ORG_WEBAPP,
                            Message = DateTime.UtcNow.ToString()
                        };

                        var messageContent = JsonConvert.SerializeObject(statusMessage);

                        stream.WriteString(messageContent);

                        if (!firstPingDone)
                        {
                            _logger.LogInformation("First ping sent.");

                            firstPingDone = true;
                        }

                        proceed = !cancellationToken.IsCancellationRequested;

                        if (proceed)
                        {
                            Task.Delay(2000, cancellationToken).Wait(cancellationToken);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        proceed = false;
                    }
                    catch (Exception)
                    {
                        // TODO: notify and close application properly
                        proceed = false;
                    }
                }
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
