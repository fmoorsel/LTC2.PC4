﻿using LTC2.Shared.Utils.Bootstrap.Interfaces;

namespace LTC2.Desktopclients.WindowsClient.Interfaces
{
    public interface IMainServiceTask : IServiceTask
    {
        public EventHandler OnReady { get; set; }
    }
}
