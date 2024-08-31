namespace LTC2.Shared.Messaging.Interfaces
{
    public enum MessageType
    {
        Text = 0, Binary = 1
    }

    public enum MessagePriority
    {
        Highest = 0,
        High = 1,
        MediumHigh = 2,
        Medium = 3,
        MediumLow = 4,
        Low = 5,
        Lowest = 6
    };

    public interface IMessage
    {
        public MessageType Type { get; set; }

        public string Id { get; set; }

        public string Group { get; set; }

        public MessagePriority Priority { get; set; }

        public object Payload { get; set; }

        public void Acknowlegde();
    }
}
