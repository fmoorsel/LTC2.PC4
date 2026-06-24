using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using LTC2.Desktopclients.AvaloniaClient.Models;
using LTC2.Desktopclients.AvaloniaClient.Services;
using LTC2.Shared.Models.Interprocess;
using LTC2.Shared.Utils.Bootstrap.Interfaces;
using LTC2.Shared.Utils.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LTC2.Desktopclients.AvaloniaClient.ServiceTasks
{
    public abstract class AbstractInitMonitorTask : IServiceTask
    {
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;
        private Task _executionTask;

        protected readonly AppSettings _appSettings;
        private readonly ILogger<AbstractInitMonitorTask> _logger;
        private readonly StatusNotifier _statusNotifier;

        public AbstractInitMonitorTask(
            AppSettings appSettings,
            StatusNotifier statusNotifier,
            ILogger<AbstractInitMonitorTask> logger)
        {
            _appSettings = appSettings;
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

                    ProcessorLoop(pipeName, _cancellationToken);

                }, TaskCreationOptions.LongRunning);
            }

            return Task.CompletedTask;
        }

        private void ProcessorLoop(string pipeName, CancellationToken cancellationToken)
        {
            var proceed = !cancellationToken.IsCancellationRequested;
            var pingDelta = _appSettings.PingDeltaInSeconds > 0 ? _appSettings.PingDeltaInSeconds : 10;
            var waitTime = 1000 * (StatusMessage.PING_DELTA_STARTUP_SLACK + 1) * pingDelta;

            using (var pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances))
            {
                _logger.LogInformation($"Pipe server started: {pipeName} {GetType().Name}");

                var connected = pipeServer.WaitForConnectionEx(waitTime, cancellationToken);

                if (pipeServer.IsConnected)
                {
                    _logger.LogInformation($"Pipe server client connected: {pipeName} {GetType().Name}");

                    var stream = new StreamString(pipeServer);

                    while (proceed)
                    {
                        try
                        {
                            var messageContent = stream.ReadString();
                            var statusMessage = JsonConvert.DeserializeObject<StatusMessage>(messageContent);

                            _statusNotifier.Notify(statusMessage);

                            proceed = !cancellationToken.IsCancellationRequested;
                        }
                        catch (OperationCanceledException)
                        {
                            proceed = false;
                        }
                        catch (Exception)
                        {
                            var statusMessage = new StatusMessage()
                            {
                                Status = StatusMessage.STATUS_FATAL,
                                Origin = GetOrigin(),
                                Message = "broken pipe"
                            };

                            _statusNotifier.Notify(statusMessage);

                            proceed = false;
                        }
                    }
                }
                else
                {
                    var statusMessage = new StatusMessage()
                    {
                        Status = StatusMessage.STATUS_FATAL,
                        Origin = GetOrigin(),
                        Message = "no connection"
                    };

                    _statusNotifier.Notify(statusMessage);
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
            var pipeNameParameter = GetPipeNameParameter();

            if (pipeNameParameter != null)
            {
                var parameters = pipeNameParameter.Split(' ');

                foreach (var parameter in parameters)
                {
                    const string pipeParToken = "pipe:";
                    if (parameter.ToLower().StartsWith(pipeParToken))
                    {
                        return parameter.Substring(pipeParToken.Length);
                    }
                }
            }

            return null;
        }

        protected abstract string GetPipeNameParameter();
        protected abstract string GetOrigin();
    }

    public static class NamedPipeExtensions
    {
        public static bool WaitForConnectionEx(this NamedPipeServerStream stream, int waitTime, CancellationToken cancellationToken)
        {
            var signal = new AutoResetEvent(false);
            var retryCount = 5;

            Exception e = null;

            while (retryCount > 0 && !stream.IsConnected)
            {
                stream.BeginWaitForConnection(ar =>
                {
                    try
                    {
                        stream.EndWaitForConnection(ar);
                    }
                    catch (Exception er)
                    {
                        e = er;
                    }

                    signal.Set();
                }, null);

                signal.WaitOne(waitTime);

                if (e != null)
                {
                    throw e;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    retryCount = 0;
                }

                retryCount--;
            }

            return stream.IsConnected;
        }
    }
}
