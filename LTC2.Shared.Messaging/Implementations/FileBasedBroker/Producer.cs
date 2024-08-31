using LTC2.Shared.Messaging.Exceptions;
using LTC2.Shared.Messaging.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;

namespace LTC2.Shared.Messaging.Implementations.FileBasedBroker
{
    public class Producer : IProducer
    {
        private readonly ILogger<Producer> _logger;

        public int MaxGroupCount { get; private set; } = 15;

        public Producer(ILogger<Producer> logger)
        {
            _logger = logger;
        }

        public Connection Connection { get; set; }

        public ITarget Target { get; set; }

        public string Id { get; set; } = Guid.NewGuid().ToString();

        public void Produce(IMessage message)
        {
            var path = Path.Combine(Connection.Folder, Target.Name);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            if (!string.IsNullOrEmpty(message.Group) && MaxGroupCount > 0)
            {
                var filesInGroup = Directory.GetFiles(path, $"*{message.Group}*");
                var filesInGroupCount = filesInGroup.Length;

                if (filesInGroupCount > MaxGroupCount)
                {
                    _logger.LogWarning($"Number of pending requests in group {message.Group} exceeded maximum of {MaxGroupCount}, message dropped.");

                    return;
                }
                else if (filesInGroupCount > 0)
                {
                    var fileNames = filesInGroup.Select(f => Path.GetFileName(f));
                    var currentHighest = (MessagePriority)fileNames.Min(f => (MessagePriority)Convert.ToInt32(f[2].ToString()));
                    var isHigherThenCurrentPrio = message.Priority < currentHighest;

                    if (!isHigherThenCurrentPrio)
                    {
                        var prioDifference = Math.Min(MessagePriority.Lowest - message.Priority, 2);

                        message.Priority = message.Priority + prioDifference;
                    }
                }
            }

            var prefix = message.Type == MessageType.Text ? FolderMonitor.TextMessagePrefix : FolderMonitor.BinaryMessagePrefix;
            var name = $"{prefix.Replace($"_{(int)MessagePriority.Medium}", $"_{(int)message.Priority}")}{message.Id}";
            var fileName = Path.Combine(path, name);

            if (File.Exists(fileName))
            {
                throw new BadMessageException($"File already exists: {fileName}");
            }
            else
            {
                if (message.Type == MessageType.Text)
                {
                    File.WriteAllText(fileName, message.Payload.ToString());
                }
                else
                {
                    if (message.Payload is byte[])
                    {
                        File.WriteAllBytes(fileName, (byte[])message.Payload);
                    }
                    else
                    {
                        throw new BadMessageException($"Payload should be of {typeof(byte[]).Name} for binary messages");
                    }
                }

            }
        }
    }
}
