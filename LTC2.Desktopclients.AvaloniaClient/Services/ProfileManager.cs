using System.Collections.Generic;
using System.Linq;
using LTC2.Shared.Models.Desktop;
using LTC2.Shared.Models.Interprocess;
using LTC2.Shared.Repositories.Interfaces;

namespace LTC2.Desktopclients.AvaloniaClient.Services
{
    public class ProfileManager
    {
        private bool? _hasMultipleProfiles;
        private Profile _profile;
        private readonly IDesktopProfileRepository _desktopProfileRepository;
        private readonly StatusNotifier _statusNotifier;

        public Profile Profile
        {
            get => _profile;
            set
            {
                _profile = value;
                _statusNotifier.Notify(new StatusMessage { Status = StatusMessage.STATUS_PROFILESELECTED });
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

        public ProfileManager(IDesktopProfileRepository desktopProfileRepository, StatusNotifier statusNotifier)
        {
            _desktopProfileRepository = desktopProfileRepository;
            _statusNotifier = statusNotifier;
        }

        public List<Profile> GetProfiles()
        {
            return _desktopProfileRepository.GetProfiles().OrderBy(p => $"{p.Name} - ({p.AthleteId})").ToList();
        }
    }
}
