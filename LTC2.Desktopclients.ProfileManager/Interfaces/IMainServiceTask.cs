using LTC2.Shared.Utils.Bootstrap.Interfaces;

namespace LTC2.Desktopclients.ProfileManager.Interfaces
{
    public interface IMainServiceTask : IServiceTask
    {
        public EventHandler OnReady { get; set; }
    }
}
