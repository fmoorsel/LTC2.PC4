using LTC2.Desktopclients.WindowsClient.Models;
using LTC2.Shared.StravaConnector.Models;
using Newtonsoft.Json;
using System.Diagnostics;

namespace LTC2.Desktopclients.WindowsClient.Services
{
    public class MultiSportManager
    {
        private readonly AppSettings _appSettings;

        private readonly string _defaultsFile = "defaults.json";

        private List<StravaActivityType> _currentActivityTypes;

        public MultiSportManager(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public string AthleteId { get; set; }

        public bool RunInMultiSportMode { get; set; }

        public List<StravaActivityType> CurrentActivityTypes 
        { 
            get
            {
                if (_currentActivityTypes == null)
                {
                    _currentActivityTypes = GetActivityTypesForAthlete();
                }

                return _currentActivityTypes;
            }
        
            set
            {
                _currentActivityTypes = value;
            } 
        
        }


        public void RefreshCurrentActivityTypes()
        {
            _currentActivityTypes = GetActivityTypesForAthlete();
        }

        public List<StravaActivityType> GetActivityTypesForAthlete(string athleteId = null)
        {
            var athlete = athleteId ?? AthleteId;

            EnsureActivtyTypes(athlete);
            
            var activityTypesFile = Path.Combine(_appSettings.MultiSportFolder, $"{athlete}.json");
            var activityTypesAsJson = File.ReadAllText(activityTypesFile);
            
            return JsonConvert.DeserializeObject<List<StravaActivityType>>(activityTypesAsJson);
        }

        public List<ActivityTypeDescription> GetActivityTypes()
        {
            var processModule = Process.GetCurrentProcess().MainModule;
            var folder = Path.Combine(Path.GetDirectoryName(processModule?.FileName), "Resources");

            var activitiesFile = Path.Combine(folder, $"activities.json");
            var content = File.ReadAllText(activitiesFile);

            return JsonConvert.DeserializeObject<List<ActivityTypeDescription>>(content);
        }

        public void SaveActivityTypesForAthlete(string athleteId = null, List<StravaActivityType> activityTypes = null)
        {
            var athlete = athleteId ?? AthleteId;
            var toSave = activityTypes ?? CurrentActivityTypes;
            var activityTypesFile = Path.Combine(_appSettings.MultiSportFolder, $"{athlete}.json");

            var asJson = JsonConvert.SerializeObject(toSave);

            File.WriteAllText(activityTypesFile, asJson);
        }

        public bool IsMultiSportDefault
        {
            get
            {
                EnsureDefaults();

                var defaultsAsJson = File.ReadAllText(Path.Combine(_appSettings.MultiSportFolder, _defaultsFile));

                var defaults = JsonConvert.DeserializeObject<MultiSportDefaults>(defaultsAsJson);

                return defaults.IsDefault;
            }
        }

        public void WriteDefaults(bool isDefault, string source)
        {
            EnsureDefaults();

            var defaultsFile = Path.Combine(_appSettings.MultiSportFolder, _defaultsFile);

            var defaultsAsJson = File.ReadAllText(defaultsFile);
            var defaults = JsonConvert.DeserializeObject<MultiSportDefaults>(defaultsAsJson);

            defaults.IsDefault = isDefault;
            defaults.Source = source;

            File.WriteAllText(defaultsFile, JsonConvert.SerializeObject(defaults));
        }

        public bool IsSourceDefault(string source)
        {
            EnsureDefaults();

            var defaultsAsJson = File.ReadAllText(Path.Combine(_appSettings.MultiSportFolder, _defaultsFile));

            var defaults = JsonConvert.DeserializeObject<MultiSportDefaults>(defaultsAsJson);

            return defaults.Source == source;
        }

        private void EnsureDefaults()
        {
            if (!Directory.Exists(_appSettings.MultiSportFolder))
            {
                Directory.CreateDirectory(_appSettings.MultiSportFolder);
            }

            var defaultsFile = Path.Combine(_appSettings.MultiSportFolder, _defaultsFile);

            if (!File.Exists(defaultsFile))
            {
                var defaults = new MultiSportDefaults();

                File.WriteAllText(defaultsFile, JsonConvert.SerializeObject(defaults));
            }
        }

        private void EnsureActivtyTypes(string athleteId)
        {
            if (!Directory.Exists(_appSettings.MultiSportFolder))
            {
                Directory.CreateDirectory(_appSettings.MultiSportFolder);
            }

            var defaultsFile = Path.Combine(_appSettings.MultiSportFolder, $"{athleteId}.json");

            if (!File.Exists(defaultsFile))
            {
                var defaultTypes = new List<StravaActivityType>()
                {
                    StravaActivityType.Ride,
                    StravaActivityType.EBikeRide,
                    StravaActivityType.MountainBikeRide,
                    StravaActivityType.Velomobile,
                    StravaActivityType.GravelRide,
                    StravaActivityType.EMountainBikeRide,
                    StravaActivityType.Run,
                    StravaActivityType.Walk,
                    StravaActivityType.Hike,
                    StravaActivityType.TrailRun
                };

                File.WriteAllText(defaultsFile, JsonConvert.SerializeObject(defaultTypes));
            }
        }
    }
}
