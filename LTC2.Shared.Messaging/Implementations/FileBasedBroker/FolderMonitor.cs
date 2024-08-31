using LTC2.Shared.Messaging.Implementations.FileBasedBroker.Interfaces;
using LTC2.Shared.Messaging.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LTC2.Shared.Messaging.Implementations.FileBasedBroker
{
    public class FolderMonitor : IFolderMonitor
    {
        private static string _messagePrefixBase = $"t_";
        private static string _messagePrefix = $"{_messagePrefixBase}{(int)MessagePriority.Medium}";

        public static string TextMessagePrefix = _messagePrefix + "t";
        public static string BinaryMessagePrefix = _messagePrefix + "b";


        private readonly EventWaitHandle _fileEventWatcher;
        private readonly FileSystemWatcher _fileWatcher;

        private readonly string _folder;
        private readonly ILogger<FolderMonitor> _logger;

        private Task _executionTask;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;

        private bool _isStarted;

        private object _lockObject = new object();

        private readonly string _contentMask = "t_*";

        private List<Consumer> _listenTargets;

        public FolderMonitor(ILogger<FolderMonitor> logger, string folder)
        {
            _folder = folder;
            _logger = logger;

            _listenTargets = new List<Consumer>();

            _fileEventWatcher = new EventWaitHandle(false, EventResetMode.AutoReset);
            _fileWatcher = new FileSystemWatcher();
        }

        public void Start()
        {
            if (!_isStarted)
            {
                _fileWatcher.Path = _folder;
                _fileWatcher.IncludeSubdirectories = true;
                _fileWatcher.Filter = _contentMask;

                _fileWatcher.Created += OnFileCreatedInOfflineFolder;

                _fileWatcher.EnableRaisingEvents = true;

                _cancellationTokenSource = new CancellationTokenSource();
                _cancellationToken = _cancellationTokenSource.Token;

                _executionTask = Task.Factory.StartNew(() =>
                {

                    Task.Delay(2000).Wait();

                    ProccesorLoop(_cancellationToken);

                }, TaskCreationOptions.LongRunning);

            }

            _isStarted = true;
        }

        private void ProccesorLoop(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"File monitor loop for {_folder} starting");

            var proceed = !_cancellationToken.IsCancellationRequested;

            CleanUp();

            while (proceed)
            {
                BrowseFolder();

                if (!_fileEventWatcher.WaitOne(120000))
                {
                    _logger.LogInformation("Waiting for file next cycle started");
                }

                proceed = !_cancellationToken.IsCancellationRequested;
            }

            _logger.LogInformation($"File monitor loop for {_folder} stopped");
        }

        private void BrowseFolder()
        {
            var found = true;

            while (found)
            {
                Consumer[] targets;

                lock (_lockObject)
                {
                    targets = _listenTargets.ToArray();
                }

                found = false;

                foreach (var consumer in targets)
                {
                    var targetFolder = Path.Combine(_folder, consumer.Target.Name);

                    if (Directory.Exists(targetFolder))
                    {
                        var targetFiles = Directory.GetFiles(targetFolder, _contentMask);

                        if (targetFiles.Length > 0)
                        {
                            Array.Sort(targetFiles);

                            var fileName = targetFiles[0];

                            found = true;

                            RemoveTarget(consumer);

                            try
                            {
                                var onlyFileName = Path.GetFileName(fileName);
                                var onlyFolderName = Path.GetDirectoryName(fileName);
                                var prefix = onlyFileName.Substring(0, _messagePrefix.Length + 1);
                                var priority = (MessagePriority)(Convert.ToInt32(prefix.Substring(_messagePrefix.Length - 1, 1)));
                                var id = onlyFileName.Substring(_messagePrefix.Length + 1);
                                var tempFileName = Path.Combine(onlyFolderName, $"{Path.GetFileName(fileName).Replace(_messagePrefixBase, "p_")}.$$$");
                                var isTextMessage = prefix.Substring(TextMessagePrefix.Length - 1) == TextMessagePrefix.Substring(TextMessagePrefix.Length - 1);

                                var payLoad = GetMessagePayload(isTextMessage, fileName, tempFileName);

                                var message = new Message()
                                {
                                    Type = isTextMessage ? MessageType.Text : MessageType.Binary,
                                    Priority = priority,
                                    Id = id,
                                    Connection = (Connection)consumer.Connection,
                                    Consumer = consumer,
                                    Target = consumer.Target,
                                    FileName = tempFileName,
                                    Payload = payLoad
                                };

                                Task.Run(async () => await consumer.Callback(message));

                            }
                            catch (Exception e)
                            {
                                _logger.LogWarning(e, $"Unable to process message in that arrived on target {consumer.Target.Name} due to {e.Message}");

                                AddTarget(consumer);
                            }
                        }
                    }
                }
            }
        }

        private object GetMessagePayload(bool isTextMessage, string fileName, string tempFileName)
        {
            var cnt = 0;
            do
            {
                try
                {
                    if (isTextMessage)
                    {
                        var payLoad = File.ReadAllText(fileName); ;

                        File.Move(fileName, tempFileName);

                        return payLoad;
                    }
                    else
                    {
                        var payLoad = File.ReadAllBytes(fileName);

                        File.Move(fileName, tempFileName);

                        return payLoad;
                    }
                }
                catch (Exception e)
                {
                    Task.Delay(cnt * 250).Wait();

                    if (cnt >= 5)
                    {
                        _logger.LogWarning(e, $"Unable to get payload {fileName}, due to {e.Message}");

                        throw;
                    }
                }

                cnt++;
            }
            while (true);
        }
        private void OnFileCreatedInOfflineFolder(object source, FileSystemEventArgs e)
        {
            _logger.LogDebug("File event!");

            _fileEventWatcher.Set();
        }

        public void Stop()
        {
            _logger.LogInformation($"About to stop file monitor loop for {_folder}");

            _cancellationTokenSource.Cancel();

            _fileEventWatcher.Set();

            _executionTask.Wait();

            CleanUp();

            _logger.LogInformation($"File monitor loop for {_folder} stopped");
        }


        private void CleanUp()
        {
            var folders = Directory.GetDirectories(_folder);

            foreach (var folder in folders)
            {
                var files = Directory.GetFiles(folder, "p_*.$$$");

                foreach (var file in files)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning(e, $"Unable to delete {file}, due to {e.Message}");
                    }
                }
            }
        }

        public void AddTarget(Consumer consumer)
        {
            lock (_lockObject)
            {
                if (_listenTargets.FirstOrDefault(l => l == consumer) == null)
                {
                    _listenTargets.Add(consumer);
                }
            }
        }

        public void Acknowledge(Message message)
        {
            try
            {
                File.Delete(message.FileName);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"Unable to remove message: {message} {e.Message}");
            }

            AddTarget(message.Consumer);

            _fileEventWatcher.Set();
        }

        public void RemoveTarget(Consumer consumer)
        {
            lock (_lockObject)
            {
                _listenTargets = _listenTargets.Where(l => l != consumer).ToList();
            }
        }
    }
}
