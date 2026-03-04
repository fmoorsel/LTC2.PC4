using LTC2.Shared.Models.Desktop;
using LTC2.Shared.Models.Interprocess;
using LTC2.Shared.Repositories.Interfaces;

namespace LTC2.Desktopclients.WindowsClient.Services
{
    public class ProfileManager
    {
        private readonly StatusNotifier _statusNotifier;


        public ProfileManager(IDesktopProfileRepository desktopProfileRepository, StatusNotifier statusNotifier)
        {
            _desktopProfileRepository = desktopProfileRepository;
            _statusNotifier = statusNotifier;
        }

        private bool? _hasMultipleProfiles;
        private Profile _profile;

        public Profile Profile
        {

            get
            {
                return _profile;
            }

            set
            {
                _profile = value;

                var notification = new StatusMessage
                {
                    Status = StatusMessage.STATUS_PROFILESELECTED
                };

                _statusNotifier.Notify(notification);
            }
        }

        public bool HasMultipleProfiles
        {
            get
            {
                if (!_hasMultipleProfiles.HasValue)
                {
                    _hasMultipleProfiles = _desktopProfileRepository.GetProfiles().ToList().Count > 1;
                }

                return _hasMultipleProfiles.Value;
            }
        }

        private readonly IDesktopProfileRepository _desktopProfileRepository;


        public List<Profile> GetProfiles()
        {
            return _desktopProfileRepository.GetProfiles().OrderBy(p => $"{p.Name} - ({p.AthleteId})").ToList();
        }


    }
}
