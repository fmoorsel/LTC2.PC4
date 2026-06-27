using LTC2.Desktopclients.AvaloniaProfileManager.Models;
using LTC2.Shared.Models.Interprocess;
using System;
using System.Collections.Generic;

namespace LTC2.Desktopclients.AvaloniaProfileManager.Services
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

        private readonly object _notifyLock = new object();
        private readonly Dictionary<string, KeepAliveStatus> _keepAliveStatus;
        private readonly int _pingDelta;
        private bool _initialized;
        private readonly AppSettings _appSettings;

        public StatusNotifier(AppSettings appSettings)
        {
            _appSettings = appSettings;
            _keepAliveStatus = new Dictionary<string, KeepAliveStatus>();
            _pingDelta = _appSettings.PingDeltaInSeconds > 0 ? _appSettings.PingDeltaInSeconds : 10;
        }

        public void Initialize()
        {
            lock (_notifyLock)
            {
                if (!_initialized)
                {
                    if (_appSettings.MonitoredComponent != null)
                    {
                        var components = _appSettings.MonitoredComponent.Split(',');

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

                    _initialized = true;
                }
            }
        }

        public void CheckKeepAliveStatuses()
        {
            var fatals = new List<StatusMessage>();

            lock (_notifyLock)
            {
                if (!_initialized)
                {
                    return;
                }

                foreach (var key in _keepAliveStatus.Keys)
                {
                    var status = _keepAliveStatus[key];

                    if (!status.Notified)
                    {
                        var timeStamp = DateTime.SpecifyKind(DateTime.Parse(status.LastPing.Message), DateTimeKind.Utc);
                        var seenAtLeastOnce = status.SeenAtLeastOnce;
                        var pingDelta = seenAtLeastOnce ? _pingDelta : _pingDelta * StatusMessage.PING_DELTA_STARTUP_SLACK;

                        if ((DateTime.UtcNow - timeStamp).TotalSeconds > pingDelta)
                        {
                            fatals.Add(new StatusMessage()
                            {
                                Origin = status.LastPing.Origin,
                                Status = StatusMessage.STATUS_FATAL,
                                Message = "missing ping"
                            });

                            status.Notified = true;
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
                var args = new OnStatusMessageEventArguments() { Status = statusMessage };

                lock (_notifyLock)
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
                            _keepAliveStatus.Add(statusMessage.Origin, new KeepAliveStatus()
                            {
                                LastPing = statusMessage,
                                FirstPingReceived = true
                            });
                        }
                    }

                    handler(this, args);
                }
            }
        }
    }
}
