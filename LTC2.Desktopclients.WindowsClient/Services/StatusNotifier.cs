using LTC2.Desktopclients.WindowsClient.Models;
using LTC2.Shared.Models.Interprocess;

namespace LTC2.Desktopclients.WindowsClient.Services
{

    public delegate void OnStatusNotification(StatusMessage statusMessage);

    public class OnStatusMessageEventArguments : EventArgs
    {
        public StatusMessage Status { get; set; }
    }

    public class KeepAliveStatus
    {
        public bool Notified { get; set; }

        public bool SeenAtLeastOnce { get; set; }

        public bool FirstPingReceived { get; set; }

        public StatusMessage LastPing { get; set; }
    }

    public class StatusNotifier
    {
        public event EventHandler<OnStatusMessageEventArguments> OnStatusNotification;

        private readonly object NotifyLock = new object();

        private readonly Dictionary<string, KeepAliveStatus> _keepAliveStatus;

        private readonly int _pingDelta;

        public StatusNotifier(AppSettings appSettings)
        {
            _keepAliveStatus = new Dictionary<string, KeepAliveStatus>();

            if (appSettings.MonitoredComponent != null)
            {
                var components = appSettings.MonitoredComponent.Split(',');

                foreach (var component in components)
                {
                    var keepAlive = new KeepAliveStatus()
                    {
                        LastPing = new StatusMessage()
                        {
                            Origin = component,
                            Status = StatusMessage.STATUS_PING,
                            Message = DateTime.UtcNow.ToString()
                        }
                    };

                    _keepAliveStatus.Add(component, keepAlive);
                }
            }

            _pingDelta = appSettings.PingDeltaInSeconds > 0 ? appSettings.PingDeltaInSeconds : 10;
        }

        public void CheckKeepAliveStatuses()
        {
            var fatals = new List<StatusMessage>();

            lock (NotifyLock)
            {
                var keys = _keepAliveStatus.Keys;

                foreach (var key in keys)
                {
                    var status = _keepAliveStatus[key];

                    if (!status.Notified)
                    {
                        var timeStamp = DateTime.Parse(status.LastPing.Message);
                        timeStamp = DateTime.SpecifyKind(timeStamp, DateTimeKind.Utc);

                        var seenAtLeastOnce = status.SeenAtLeastOnce;
                        var pingDelta = seenAtLeastOnce ? _pingDelta : _pingDelta * StatusMessage.PING_DELTA_STARTUP_SLACK;

                        if ((DateTime.UtcNow - timeStamp).TotalSeconds > pingDelta)
                        {
                            var statusMessage = new StatusMessage()
                            {
                                Origin = status.LastPing.Origin,
                                Status = StatusMessage.STATUS_FATAL,
                                Message = "missing ping"
                            };

                            status.Notified = true;

                            fatals.Add(statusMessage);
                        }

                        status.SeenAtLeastOnce = status.FirstPingReceived;
                    }
                }
            }

            if (fatals.Count > 0)
            {
                Notify(fatals[0]);
            }
        }


        public void Notify(StatusMessage statusMessage)
        {
            var handler = OnStatusNotification;

            if (handler != null)
            {
                var onStatusMessageEventArguments = new OnStatusMessageEventArguments()
                {
                    Status = statusMessage
                };

                lock (NotifyLock)
                {
                    if (statusMessage.Status == StatusMessage.STATUS_PING)
                    {
                        if (_keepAliveStatus.ContainsKey(statusMessage.Origin))
                        {
                            _keepAliveStatus[statusMessage.Origin].FirstPingReceived = true;
                            _keepAliveStatus[statusMessage.Origin].LastPing = statusMessage;
                        }
                        else
                        {
                            var keepAliveStatus = new KeepAliveStatus()
                            {
                                LastPing = statusMessage,
                                FirstPingReceived = true
                            };

                            _keepAliveStatus.Add(statusMessage.Origin, keepAliveStatus);
                        }
                    }

                    handler(this, onStatusMessageEventArguments);
                }
            }
        }

    }
}
