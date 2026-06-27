using System;
using System.IO;
using System.Threading.Tasks;
using LTC2.Desktopclients.AvaloniaClient.Interfaces;
using LTC2.Desktopclients.AvaloniaClient.Models;

namespace LTC2.Desktopclients.AvaloniaClient.ServiceTasks
{
    public class ClearLoggingFoldersTasks : IFirstServiceTask
    {
        private readonly AppSettings _appSettings;

        public ClearLoggingFoldersTasks(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public Task ExecuteAsync()
        {
            if (_appSettings.LogFolders != null)
            {
                try
                {
                    var folders = _appSettings.LogFolders.Split(',');

                    foreach (var folder in folders)
                    {
                        if (Directory.Exists(folder))
                        {
                            var files = Directory.GetFiles(folder);

                            foreach (var file in files)
                            {
                                try
                                {
                                    File.Delete(file);
                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
            }

            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            return Task.CompletedTask;
        }
    }
}
