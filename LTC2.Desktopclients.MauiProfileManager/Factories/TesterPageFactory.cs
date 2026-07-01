using LTC2.Desktopclients.MauiProfileManager.Interfaces;
using LTC2.Desktopclients.MauiProfileManager.Pages;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace LTC2.Desktopclients.MauiProfileManager.Factories
{
    public class TesterPageFactory : ITesterPageFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public TesterPageFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public TesterPage CreateTesterPage()
        {
            return _serviceProvider.GetRequiredService<TesterPage>();
        }
    }
}
