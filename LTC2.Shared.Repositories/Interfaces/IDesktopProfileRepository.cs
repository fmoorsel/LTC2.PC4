using LTC2.Shared.Models.Desktop;
using System.Collections.Generic;

namespace LTC2.Shared.Repositories.Interfaces
{
    public interface IDesktopProfileRepository
    {
        public List<Profile> GetProfiles();

        public Profile GetProfile(string id, bool tempProfile = false);

        public bool StoreProfile(Profile profile, bool tempProfile = false);

        public bool DeleteProfile(Profile profile);

        public void RemoveAllTempProfiles();

    }
}
