using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using LTC2.Desktopclients.MauiClient.Models;
using LTC2.Shared.Models.Domain;
using Newtonsoft.Json;

namespace LTC2.Desktopclients.MauiClient.Services
{
    public class MultiSportManager
    {
        private readonly AppSettings _appSettings;
        private readonly string _defaultsFile = "defaults.json";

        private List<GenericActivityType> _currentActivityTypes;
        private List<string> _currentRwGpsActivityTypes;

        public MultiSportManager(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public string AthleteId { get; set; }
        public bool RunInMultiSportMode { get; set; }
        public string RunWithSource { get; set; }

        public List<GenericActivityType> CurrentActivityTypes
        {
            get
            {
                if (_currentActivityTypes == null) _currentActivityTypes = GetActivityTypesForAthlete();
                return _currentActivityTypes;
            }
            set => _currentActivityTypes = value;
        }

        public List<string> CurrentRwGpsActivityTypes
        {
            get
            {
                if (_currentRwGpsActivityTypes == null) _currentRwGpsActivityTypes = GetRwGpsActivityTypesForAthlete();
                return _currentRwGpsActivityTypes;
            }
            set => _currentRwGpsActivityTypes = value;
        }

        public void RefreshCurrentActivityTypes()
        {
            if (RunWithSource == "ridewithgps")
                _currentRwGpsActivityTypes = GetRwGpsActivityTypesForAthlete();
            else
                _currentActivityTypes = GetActivityTypesForAthlete();
        }

        public List<GenericActivityType> GetActivityTypesForAthlete(string athleteId = null)
        {
            var athlete = athleteId ?? AthleteId;
            EnsureActivityTypes(athlete);
            var file = Path.Combine(_appSettings.MultiSportFolder, $"{athlete}.json");
            return JsonConvert.DeserializeObject<List<GenericActivityType>>(File.ReadAllText(file));
        }

        public List<string> GetRwGpsActivityTypesForAthlete(string athleteId = null)
        {
            var athlete = athleteId ?? AthleteId;
            EnsureRwGpsActivityTypes(athlete);
            var file = Path.Combine(_appSettings.MultiSportFolder, $"{athlete}_rwgps.json");
            return JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(file));
        }

        public List<ActivityTypeDescription> GetActivityTypes()
        {
            var processModule = Process.GetCurrentProcess().MainModule;
            var folder = Path.Combine(Path.GetDirectoryName(processModule?.FileName), "Resources");
            var content = File.ReadAllText(Path.Combine(folder, "activities.json"));
            return JsonConvert.DeserializeObject<List<ActivityTypeDescription>>(content);
        }

        public List<RwGpsActivityTypeDescription> GetRwGpsActivityTypes()
        {
            var processModule = Process.GetCurrentProcess().MainModule;
            var folder = Path.Combine(Path.GetDirectoryName(processModule?.FileName), "Resources");
            var content = File.ReadAllText(Path.Combine(folder, "activities.rwgps.json"));
            return JsonConvert.DeserializeObject<List<RwGpsActivityTypeDescription>>(content);
        }

        public void SaveActivityTypesForAthlete(string athleteId = null, List<GenericActivityType> activityTypes = null)
        {
            var athlete = athleteId ?? AthleteId;
            var toSave = activityTypes ?? CurrentActivityTypes;
            File.WriteAllText(Path.Combine(_appSettings.MultiSportFolder, $"{athlete}.json"), JsonConvert.SerializeObject(toSave));
        }

        public void SaveRwGpsActivityTypesForAthlete(string athleteId = null, List<string> activityTypes = null)
        {
            var athlete = athleteId ?? AthleteId;
            var toSave = activityTypes ?? CurrentRwGpsActivityTypes;
            File.WriteAllText(Path.Combine(_appSettings.MultiSportFolder, $"{athlete}_rwgps.json"), JsonConvert.SerializeObject(toSave));
        }

        public bool IsMultiSportDefault
        {
            get
            {
                EnsureDefaults();
                var json = File.ReadAllText(Path.Combine(_appSettings.MultiSportFolder, _defaultsFile));
                return JsonConvert.DeserializeObject<MultiSportDefaults>(json).IsDefault;
            }
        }

        public void WriteDefaults(bool isDefault)
        {
            EnsureDefaults();
            var defaultsFile = Path.Combine(_appSettings.MultiSportFolder, _defaultsFile);
            var defaults = JsonConvert.DeserializeObject<MultiSportDefaults>(File.ReadAllText(defaultsFile));
            defaults.IsDefault = isDefault;
            File.WriteAllText(defaultsFile, JsonConvert.SerializeObject(defaults));
        }

        private void EnsureDefaults()
        {
            if (!Directory.Exists(_appSettings.MultiSportFolder))
                Directory.CreateDirectory(_appSettings.MultiSportFolder);

            var defaultsFile = Path.Combine(_appSettings.MultiSportFolder, _defaultsFile);
            if (!File.Exists(defaultsFile))
                File.WriteAllText(defaultsFile, JsonConvert.SerializeObject(new MultiSportDefaults()));
        }

        private void EnsureActivityTypes(string athleteId)
        {
            if (!Directory.Exists(_appSettings.MultiSportFolder))
                Directory.CreateDirectory(_appSettings.MultiSportFolder);

            var typesFile = Path.Combine(_appSettings.MultiSportFolder, $"{athleteId}.json");
            if (!File.Exists(typesFile))
            {
                var defaultTypes = new List<GenericActivityType>()
                {
                    GenericActivityType.Ride, GenericActivityType.EBikeRide,
                    GenericActivityType.MountainBikeRide, GenericActivityType.Velomobile,
                    GenericActivityType.GravelRide, GenericActivityType.EMountainBikeRide,
                    GenericActivityType.Run, GenericActivityType.Walk,
                    GenericActivityType.Hike, GenericActivityType.TrailRun
                };
                File.WriteAllText(typesFile, JsonConvert.SerializeObject(defaultTypes));
            }
        }

        private void EnsureRwGpsActivityTypes(string athleteId)
        {
            if (!Directory.Exists(_appSettings.MultiSportFolder))
                Directory.CreateDirectory(_appSettings.MultiSportFolder);

            var typesFile = Path.Combine(_appSettings.MultiSportFolder, $"{athleteId}_rwgps.json");
            if (!File.Exists(typesFile))
            {
                var defaultTypes = new List<string>()
                {
                    "cycling:generic", "cycling:road", "cycling:gravel", "cycling:mountain",
                    "cycling:commute", "cycling:cyclocross", "cycling:hand_cycling",
                    "cycling:recumbent", "cycling:indoor", "running:generic", "running:road",
                    "running:trail", "running:indoor", "walking:generic", "walking:hiking",
                    "walking:indoor", "walking:speed", "other:generic", "unknown:generic"
                };
                File.WriteAllText(typesFile, JsonConvert.SerializeObject(defaultTypes));
            }
        }
    }
}
