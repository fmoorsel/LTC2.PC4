using System;
using LTC2.Shared.Utils.Bootstrap.Interfaces;

namespace LTC2.Desktopclients.AvaloniaProfileManager.Interfaces
{
    public interface IMainServiceTask : IServiceTask
    {
        EventHandler OnReady { get; set; }
    }
}
