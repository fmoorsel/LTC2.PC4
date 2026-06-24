using System;
using System.Threading.Tasks;
using Avalonia;
using LTC2.Desktopclients.AvaloniaClient.Interfaces;
using LTC2.Desktopclients.AvaloniaClient.Services;

namespace LTC2.Desktopclients.AvaloniaClient.ServiceTasks
{
    public class StartAvalonia : IMainServiceTask
    {
        public EventHandler OnReady { get; set; }

        private readonly ApplicationManager _applicationManager;

        public StartAvalonia(ApplicationManager applicationManager)
        {
            _applicationManager = applicationManager;
        }

        public Task ExecuteAsync()
        {
            App.OnReady += OnReady;

            try
            {
                _applicationManager.AppBuilder.StartWithClassicDesktopLifetime(Environment.GetCommandLineArgs());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            return Task.CompletedTask;
        }
    }
}
