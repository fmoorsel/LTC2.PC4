using LTC2.Desktopclients.WindowsClient.Models;
using LTC2.Desktopclients.WindowsClient.Services;
using LTC2.Shared.Models.Interprocess;
using LTC2.Shared.Utils.Bootstrap.Interfaces;
using LTC2.Shared.Utils.Utils;
using Newtonsoft.Json;
using System.IO.Pipes;

namespace LTC2.Desktopclients.WindowsClient.ServiceTasks
{
    public abstract class AbstractInitMonitorTask : IServiceTask
    {
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;

        private Task _executionTask;

        protected readonly AppSettings _appSettings;

        private readonly StatusNotifier _statusNotifier;

        public AbstractInitMonitorTask(AppSettings appSettings, StatusNotifier statusNotifier)
        {
            _appSettings = appSettings;
            _statusNotifier = statusNotifier;
        }

        public Task ExecuteAsync()
        {
            var pipeName = GetPipname();

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
            var waitTime = 1000 * (StatusMessage.PING_DELTA_STARTUP_SLACK + 1) * (_appSettings.PingDeltaInSeconds > 0 ? _appSettings.PingDeltaInSeconds : 10);

            using (var pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.In))
            {
                var connected = pipeServer.WaitForConnectionEx(waitTime, cancellationToken);

                if (pipeServer.IsConnected)
                {
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

        private string GetPipname()
        {
            var pipeNameParameter = GetPipnameParameter();

            if (pipeNameParameter != null)
            {
                var parameters = pipeNameParameter.Split(' ');

                foreach (var parameter in parameters)
                {
                    var pipeParToken = "pipe:";
                    if (parameter.ToLower().StartsWith(pipeParToken))
                    {
                        return parameter.Substring(pipeParToken.Length);
                    }
                }
            }

            return null;
        }

        protected abstract string GetPipnameParameter();

        protected abstract string GetOrigin();
    }

    public static class NamedPipeExtions
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
                    throw e; // rethrow exception
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
