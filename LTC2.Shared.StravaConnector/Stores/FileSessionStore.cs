using LTC2.Shared.Models.Domain;
using LTC2.Shared.Models.Settings;
using LTC2.Shared.StravaConnector.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LTC2.Shared.StravaConnector.Stores
{
    public class FileSessionStore : ISessionStore
    {
        private readonly ILogger<FileSessionStore> _logger;
        private readonly GenericSettings _genericSettings;

        public FileSessionStore(ILogger<FileSessionStore> logger, GenericSettings genericSettings)
        {
            _logger = logger;
            _genericSettings = genericSettings;

        }

        public async Task<Session> RetrieveAsync(long athleteId, Session currentSession = null)
        {
            var result = currentSession ?? new Session();

            try
            {
                if (athleteId > 0)
                {
                    var folder = Path.GetDirectoryName(_genericSettings.SessionsFolder);

                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    var fileName = Path.Combine(_genericSettings.SessionsFolder, $"s{athleteId}");

                    var json = await File.ReadAllTextAsync(fileName);

                    result = JsonConvert.DeserializeObject<Session>(json);
                }
                else
                {
                    _logger.LogWarning($"Missing athlete ID, not retrieving refresh token");
                }

            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unable to retrieve refresh token for {athleteId}: {e.Message}");
            }

            return result;
        }
        public Session Retrieve(long athleteId, Session currentSession = null)
        {
            var result = currentSession ?? new Session();

            try
            {
                if (athleteId > 0)
                {
                    var fileName = Path.Combine(_genericSettings.SessionsFolder, $"s{athleteId}");

                    var json = File.ReadAllText(fileName);

                    result = JsonConvert.DeserializeObject<Session>(json);
                }
                else
                {
                    _logger.LogWarning($"Missing athlete ID, not retrieving refresh token");
                }

            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unable to retrieve refresh token for {athleteId}: {e.Message}");
            }

            return result;
        }

        public void Store(Session session)
        {
            int maxRetry = 3;

            for (int i = 0; i < maxRetry; i++)
            {
                try
                {
                    if (session.AthleteId > 0 && session.RefreshToken != null)
                    {
                        var serializer = new JsonSerializer();
                        var fileName = Path.Combine(_genericSettings.SessionsFolder, $"s{session.AthleteId}");

                        if (!Directory.Exists(_genericSettings.SessionsFolder))
                        {
                            Directory.CreateDirectory(_genericSettings.SessionsFolder);
                        }

                        using (StreamWriter sw = new StreamWriter(fileName))
                        using (JsonWriter writer = new JsonTextWriter(sw))
                        {
                            serializer.Serialize(writer, session);
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"Missing refresh token or athlete ID, not storing refresh token for {session.AthleteId}");
                    }

                    break;
                }
                catch (Exception e)
                {
                    if (i + 1 == maxRetry)
                    {
                        _logger.LogError(e, $"Unable to store refresh token for {session.AthleteId}: {e.Message}");
                    }
                    else
                    {
                        _logger.LogWarning(e, $"Unable to store refresh token for {session.AthleteId} but a retry is performaned: {e.Message}");
                    }

                }
            }
        }
    }
}
