using LTC2.Shared.Models.Settings;
using LTC2.Shared.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace LTC2.Webapps.MainApp.Services
{
    public class ProfileManager
    {
        private readonly StravaHttpProxySettings _stravaHttpProxySettings;
        private readonly IDesktopProfileRepository _desktopProfileRepository;
        private readonly ILogger<ProfileManager> _logger;

        public string CurrentProfile { get; set; }


        public ProfileManager(
            StravaHttpProxySettings stravaHttpProxySettings,
            IDesktopProfileRepository desktopProfileRepository,
            ILogger<ProfileManager> logger)
        {
            _stravaHttpProxySettings = stravaHttpProxySettings;
            _desktopProfileRepository = desktopProfileRepository;
            _logger = logger;

            CurrentProfile = _stravaHttpProxySettings.ClientId;
        }

        public bool ActivateProfile(string profile, bool test)
        {
            var desktopProfile = _desktopProfileRepository.GetProfile(profile, test);

            if (desktopProfile != null)
            {
                _stravaHttpProxySettings.ClientId = desktopProfile.StravaID;
                _stravaHttpProxySettings.ClientSecret = desktopProfile.StravaClientSecret;

                CurrentProfile = _stravaHttpProxySettings.ClientId;

                return true;
            }

            return false;
        }
    }
}
