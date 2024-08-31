using LTC2.Shared.Models.Desktop;
using LTC2.Shared.Repositories.Interfaces;

namespace LTC2.Desktopclients.WindowsClient.Services
{
    public class ProfileManager
    {
        private bool? _hasMultipleProfiles;

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

        private readonly IDesktopProfileRepository _desktopProfileRepository;

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
