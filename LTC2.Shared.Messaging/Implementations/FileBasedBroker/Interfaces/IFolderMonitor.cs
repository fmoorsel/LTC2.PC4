namespace LTC2.Shared.Messaging.Implementations.FileBasedBroker.Interfaces
{
    public interface IFolderMonitor
    {
        public void Start();

        public void AddTarget(Consumer consumer);

        public void RemoveTarget(Consumer consumer);

        public void Acknowledge(Message message);

        public void Stop();
    }
}
