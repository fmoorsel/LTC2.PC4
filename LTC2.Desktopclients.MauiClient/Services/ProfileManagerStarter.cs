using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using LTC2.Desktopclients.MauiClient.Models;

namespace LTC2.Desktopclients.MauiClient.Services
{
    public class ProfileManagerStarter
    {
        private readonly AppSettings _appSettings;

        public ProfileManagerStarter(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public void Start()
        {
            if (_appSettings.ProfileApp != null)
            {
                var workingDirectory = Path.GetDirectoryName(_appSettings.ProfileApp);

                var startInfo = new ProcessStartInfo();
                startInfo.WorkingDirectory = workingDirectory;
                startInfo.FileName = _appSettings.ProfileApp;
                startInfo.CreateNoWindow = _appSettings.CalculatorNoWindow;

                var process = Process.Start(startInfo);

                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    process?.WaitForExit();
                }

                Environment.Exit(0);
            }
        }
    }
}
