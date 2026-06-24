using System.Collections.Generic;
using System.Linq;
using LTC2.Shared.Models.Desktop;
using LTC2.Shared.Repositories.Interfaces;

namespace LTC2.Desktopclients.AvaloniaClient.Services
{
    public class ProfileManager
    {
        private bool? _hasMultipleProfiles;
        private readonly IDesktopProfileRepository _desktopProfileRepository;

        public Profile Profile { get; set; }

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

        public ProfileManager(IDesktopProfileRepository desktopProfileRepository)
        {
            _desktopProfileRepository = desktopProfileRepository;
        }

        public List<Profile> GetProfiles()
        {
            return _desktopProfileRepository.GetProfiles().OrderBy(p => $"{p.Name} - ({p.AthleteId})").ToList();
        }
    }
}
