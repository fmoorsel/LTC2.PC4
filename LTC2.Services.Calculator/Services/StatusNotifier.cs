using LTC2.Shared.Models.Interprocess;
using System.Collections.Concurrent;

namespace LTC2.Services.Calculator.Services
{
    public class StatusNotifier
    {

        private readonly BlockingCollection<StatusMessage> _queue;

        public bool Enabled { get; set; }

        public StatusNotifier()
        {
            _queue = new BlockingCollection<StatusMessage>(new ConcurrentQueue<StatusMessage>(), 50);
        }


        public void SetNotification(string status, string message)
        {
            if (Enabled)
            {
                var notification = new StatusMessage()
                {
                    Status = status,
                    Origin = StatusMessage.ORG_CALCULATOR,
                    Message = message
                };

                _queue.Add(notification);
            }
        }

        public StatusMessage GetNotification()
        {
            StatusMessage message;

            var taken = _queue.TryTake(out message, 2000);

            return taken ? message : null;
        }
    }
}
