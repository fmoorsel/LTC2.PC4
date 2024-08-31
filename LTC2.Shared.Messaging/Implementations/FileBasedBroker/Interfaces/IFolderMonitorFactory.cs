namespace LTC2.Shared.Messaging.Implementations.FileBasedBroker.Interfaces
{
    public interface IFolderMonitorFactory
    {
        public IFolderMonitor CreateFolderMonitor(string folder);
    }
}
