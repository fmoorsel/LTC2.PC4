using LTC2.Desktopclients.AvaloniaProfileManager.Interfaces;
using LTC2.Desktopclients.AvaloniaProfileManager.Windows;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace LTC2.Desktopclients.AvaloniaProfileManager.Factories
{
    public class TesterWindowFactory : ITesterWindowFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public TesterWindowFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public TesterWindow CreateTesterWindow()
        {
            return _serviceProvider.GetRequiredService<TesterWindow>();
        }
    }
}
