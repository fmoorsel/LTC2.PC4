using LTC2.Shared.Models.Settings;
using LTC2.Shared.Repositories.Interfaces;
using LTC2.Shared.Utils.Bootstrap.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace LTC2.Webapps.MainApp.ServiceTasks
{
    public class InitStravaPropertiesTask : IServiceTask
    {
        private readonly StravaHttpProxySettings _stravaHttpProxySettings;
        private readonly IDesktopProfileRepository _desktopProfileRepository;
        private readonly ILogger<InitStravaPropertiesTask> _logger;

        public InitStravaPropertiesTask(
            StravaHttpProxySettings stravaHttpProxySettings,
            IDesktopProfileRepository desktopProfileRepository,
            ILogger<InitStravaPropertiesTask> logger)
        {
            _stravaHttpProxySettings = stravaHttpProxySettings;
            _desktopProfileRepository = desktopProfileRepository;
            _logger = logger;
        }

        public Task ExecuteAsync()
        {
            var profileId = GetProfileToUse();

            if (profileId != null)
            {
                var profile = _desktopProfileRepository.GetProfile(profileId);

                if (profile != null)
                {
                    _logger.LogInformation($"Using profile {profileId} with clientId {profile.StravaID}");

                    _stravaHttpProxySettings.ClientSecret = profile.StravaClientSecret;
                    _stravaHttpProxySettings.ClientId = profile.StravaID;
                }
            }

            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            return Task.CompletedTask;
        }

        private string GetProfileToUse()
        {
            var arguments = Environment.GetCommandLineArgs();

            foreach (var parameter in arguments)
            {
                var urlsParToken = "prof:";
                if (parameter.ToLower().StartsWith(urlsParToken))
                {
                    var prof = parameter.Substring(urlsParToken.Length);

                    _logger.LogDebug($"Using profile {prof}.");

                    return prof;
                }
            }

            return null;
        }
    }
}
