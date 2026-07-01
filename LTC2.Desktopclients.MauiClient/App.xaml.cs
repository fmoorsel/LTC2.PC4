using System;
using System.Threading.Tasks;
using LTC2.Desktopclients.MauiClient.Pages;
using LTC2.Desktopclients.MauiClient.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace LTC2.Desktopclients.MauiClient
{
    public partial class App : Application
    {
        private readonly IServiceProvider _services;

        public App(IServiceProvider services)
        {
            _services = services;
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            var startPage = _services.GetRequiredService<StartPage>();
            return new Window(startPage) { Title = "LTC2 Client" };
        }

        protected override void OnStart()
        {
            base.OnStart();
            var worker = _services.GetRequiredService<Worker>();
            Task.Run(() => worker.Execute());
        }
    }
}
