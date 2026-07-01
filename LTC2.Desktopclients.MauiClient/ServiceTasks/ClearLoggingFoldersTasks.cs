using System;
using System.IO;
using System.Threading.Tasks;
using LTC2.Desktopclients.MauiClient.Interfaces;
using LTC2.Desktopclients.MauiClient.Models;

namespace LTC2.Desktopclients.MauiClient.ServiceTasks
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
                            foreach (var file in Directory.GetFiles(folder))
                            {
                                try { File.Delete(file); } catch { }
                            }
                        }
                    }
                }
                catch { }
            }

            return Task.CompletedTask;
        }

        public Task StopAsync() => Task.CompletedTask;
    }
}
