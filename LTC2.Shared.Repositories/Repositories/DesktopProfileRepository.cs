using LTC2.Shared.Repositories.Interfaces;
using LTC2.Shared.Secrets.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Profile = LTC2.Shared.Models.Desktop.Profile;

namespace LTC2.Shared.Repositories.Repositories
{
    public class DesktopProfileRepository : IDesktopProfileRepository
    {
        private readonly ILogger<DesktopProfileRepository> _logger;
        private readonly ISecretsVault _vault;

        private readonly string _secretsType;

        public DesktopProfileRepository(ISecretsVault vault, ILogger<DesktopProfileRepository> logger)
        {
            _logger = logger;
            _vault = vault;

            _secretsType = "profile";
        }

        public bool DeleteProfile(Profile profile)
        {
            try
            {
                _vault.RemoveSecrect(_secretsType, profile.ID);

                return true;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"Unable to delete profile {profile.ID}, due to {e.Message}");
            }

            return false;
        }

        public Profile GetProfile(string id, bool tempProfile = false)
        {
            try
            {
                var asString = _vault.GetSecret(_secretsType, id, tempProfile);
                var profile = JsonConvert.DeserializeObject<Profile>(asString);

                return profile;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"Unable to get profile {id}, due to {e.Message}");

            }

            return null;
        }

        public List<Profile> GetProfiles()
        {
            var result = new List<Profile>();

            try
            {
                var profilesAsString = _vault.GetSecrects(_secretsType);

                foreach (var profile in profilesAsString)
                {
                    result.Add(JsonConvert.DeserializeObject<Profile>(profile));
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"Unable to get profiles, due to {e.Message}");

            }

            return result;
        }

        public void RemoveAllTempProfiles()
        {
            try
            {
                _vault.RemoveAllTempSecrets(_secretsType);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"Unable to remove temp profiles, due to {e.Message}");

            }
        }

        public bool StoreProfile(Profile profile, bool tempProfile = false)
        {
            try
            {
                var asString = JsonConvert.SerializeObject(profile, Formatting.None);

                _vault.StoreSecret(_secretsType, profile.ID, asString, tempProfile);

                return true;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"Unable to store profile {profile.ID}, due to {e.Message}");
            }

            return false;
        }
    }
}
