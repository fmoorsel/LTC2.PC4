using LTC2.Desktopclients.AvaloniaProfileManager.Models;
using LTC2.Shared.Models.Interprocess;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<StatusNotifier> _logger;

        public StatusNotifier(AppSettings appSettings, ILogger<StatusNotifier> logger)
        {
            _appSettings = appSettings;
            _logger = logger;
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
                                    Message = DateTime.UtcNow.ToString(),
                                    Ticks = Environment.TickCount64
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
                        var lastPingTicks = status.LastPing.Ticks;
                        var seenAtLeastOnce = status.SeenAtLeastOnce;
                        var pingDelta = seenAtLeastOnce ? _pingDelta : _pingDelta * StatusMessage.PING_DELTA_STARTUP_SLACK;
                        var nowTicks = Environment.TickCount64;
                        var elapsedSeconds = (nowTicks - lastPingTicks) / 1000.0;

                        if (elapsedSeconds > pingDelta)
                        {
                            _logger.LogWarning(
                                "CheckKeepAliveStatuses: {Origin} declared FATAL (missing ping). elapsed={ElapsedSeconds:F1}s threshold={PingDelta}s seenAtLeastOnce={SeenAtLeastOnce} lastPingTicks={LastPingTicks} nowTicks={NowTicks} lastPingRaw='{RawMessage}'",
                                key, elapsedSeconds, pingDelta, seenAtLeastOnce, lastPingTicks, nowTicks, status.LastPing.Message);

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
                            var previousPing = _keepAliveStatus[statusMessage.Origin].LastPing;

                            _logger.LogDebug(
                                "Notify: ping received for {Origin}. previousRaw='{PreviousRaw}' newRaw='{NewRaw}'",
                                statusMessage.Origin, previousPing?.Message, statusMessage.Message);

                            _keepAliveStatus[statusMessage.Origin].FirstPingReceived = true;
                            _keepAliveStatus[statusMessage.Origin].LastPing = statusMessage;
                        }
                        else
                        {
                            _logger.LogDebug("Notify: first-ever ping received for new origin {Origin}. raw='{NewRaw}'", statusMessage.Origin, statusMessage.Message);

                            _keepAliveStatus.Add(statusMessage.Origin, new KeepAliveStatus()
                            {
                                LastPing = statusMessage,
                                FirstPingReceived = true
                            });
                        }
                    }
                    else if (statusMessage.Status == StatusMessage.STATUS_FATAL)
                    {
                        _logger.LogError("Notify: STATUS_FATAL raised for {Origin}: {Message}", statusMessage.Origin, statusMessage.Message);
                    }

                    handler(this, args);
                }
            }
        }
    }
}
