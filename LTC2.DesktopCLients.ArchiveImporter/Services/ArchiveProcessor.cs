using LTC2.DesktopClients.ArchiveImporter.Models;
using LTC2.Shared.ActivityFormats.Fit.Utils;
using LTC2.Shared.ActivityFormats.Gpx.Utils;
using LTC2.Shared.ActivityFormats.Json.Utils;
using LTC2.Shared.ActivityFormats.Models;
using LTC2.Shared.ActivityFormats.Tcx.Utils;
using LTC2.Shared.Models.Settings;
using LTC2.Shared.StravaConnector.Models;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Text;

namespace LTC2.DesktopClients.ArchiveImporter.Services
{
    public delegate void ArchiveImportUpdateStatusDelegate(string status);

    public class ArchiveProcessor
    {

        private readonly AppSettings _appSettings;
        private readonly GenericSettings _genericSettings;

        private string _athleteId = string.Empty;

        public ArchiveProcessor(
            AppSettings appSettings,
            GenericSettings genericSettings)
        {
            _appSettings = appSettings;
            _genericSettings = genericSettings;
        }

        public async Task<bool> Process(string fileName, ArchiveImportUpdateStatusDelegate statusUpdater)
        {
            var result = await Task.Run(() => ProcessSync(fileName, statusUpdater));

            return result;
        }

        private bool ProcessSync(string fileName, ArchiveImportUpdateStatusDelegate statusUpdater)
        {
            try
            {
                var zipFile = MoveToTempFolder(fileName, statusUpdater);

                _athleteId = string.Empty;

                UnzipArchive(zipFile, statusUpdater);

                _athleteId = GetAthleteID();

                if (!string.IsNullOrEmpty(_athleteId))
                {
                    return ProcessActivities(statusUpdater);
                }

                return false;
            }
            catch (Exception)
            {
                if (_athleteId != string.Empty)
                {
                    RemoveArchive(_athleteId);
                }

                return false;
            }
            finally
            {
                try
                {
                    InitTempFolder(false);
                }
                catch (Exception)
                {
                    // ignore failure
                }
            }
        }

        public void RemoveArchive(string archive)
        {
            try
            {
                var parts = archive.Split('-');
                var athleteId = parts[0].Trim();

                var cacheFolder = Path.Combine(_genericSettings.CacheFolder, "Streams", athleteId);

                if (Directory.Exists(cacheFolder))
                {
                    Directory.Delete(cacheFolder, true);
                }
            }
            catch (Exception)
            {
                // ignore
            }
        }

        public List<string> GetArchives()
        {
            var result = new List<string>();
            try
            {
                var cacheFolder = Path.Combine(_genericSettings.CacheFolder, "Streams");

                var folders = Directory.GetDirectories(cacheFolder);

                foreach (var folder in folders)
                {
                    var athleteId = $"{Path.GetFileName(folder)} - {folder}";

                    result.Add(athleteId);
                }
            }
            catch (Exception)
            {
                // ignore
            }

            return result;
        }

        private string MoveToTempFolder(string fileName, ArchiveImportUpdateStatusDelegate statusUpdater)
        {
            statusUpdater("#move.file");

            var tempFolder = InitTempFolder(true);

            var onlyFileName = Path.GetFileName(fileName);
            var targetFileName = Path.Combine(tempFolder, Path.GetFileName(onlyFileName));

            File.Copy(fileName, targetFileName);

            statusUpdater("#moved.file");

            return targetFileName;
        }

        private const string WORK_FOLDER = "Work";

        private string InitTempFolder(bool create)
        {
            var tempFolder = _appSettings.TempFolder;
            var tempWorkFolder = Path.Combine(tempFolder, WORK_FOLDER);

            if (Directory.Exists(tempFolder))
            {
                Directory.Delete(tempFolder, true);
            }

            if (create)
            {
                Directory.CreateDirectory(tempWorkFolder);
            }

            return tempFolder;
        }

        private void UnzipArchive(string zipFile, ArchiveImportUpdateStatusDelegate statusUpdater)
        {
            statusUpdater("#unzip.file");

            var tempFolder = _appSettings.TempFolder;

            using (var archive = ZipFile.OpenRead(zipFile))
            {
                var count = archive.Entries.Count;
                var fileNumber = 0;

                foreach (var entry in archive.Entries)
                {
                    fileNumber++;

                    var destinationPath = Path.GetFullPath(Path.Combine(tempFolder, entry.FullName));
                    var percentage = 100 * fileNumber / count;

                    if (entry.FullName.EndsWith('/'))
                    {
                        Directory.CreateDirectory(destinationPath);
                    }
                    else
                    {
                        entry.ExtractToFile(destinationPath);
                    }

                    statusUpdater($"#unzip.file.progress {percentage}%");
                }
            }

            statusUpdater($"#unzipped.file");
        }

        private string GetAthleteID()
        {
            var tempFolder = _appSettings.TempFolder;

            var athleteFile = Path.Combine(tempFolder, "profile.csv");

            if (File.Exists(athleteFile))
            {
                var lines = File.ReadAllLines(athleteFile);
                var contentLines = lines.Skip(1).ToList();

                if (contentLines.Count >= 1)
                {
                    var line = contentLines[0];

                    var parts = CsvSplitLine(line);

                    return parts[0];
                }
            }

            return string.Empty;
        }

        private const int COL_ACT_ID = 0;
        private const int COL_ACT_TYPE = 3;
        private const int COL_ACT_FILE = 12;

        private bool ProcessActivities(ArchiveImportUpdateStatusDelegate statusUpdater)
        {
            var tempFolder = _appSettings.TempFolder;
            var tempWorkFolder = Path.Combine(tempFolder, WORK_FOLDER);

            var activitiesFile = Path.Combine(tempFolder, "activities.csv");

            if (File.Exists(activitiesFile))
            {
                var lines = File.ReadAllLines(activitiesFile);
                var contentLines = lines.Skip(1);

                var colId = COL_ACT_ID;
                var colType = COL_ACT_TYPE;
                var colFile = COL_ACT_FILE;

                var results = new Dictionary<string, LineString>();
                var supportedActivityCountType = 0;

                var lineNumbers = contentLines.ToList().Count;
                var count = 0;
                var supportedCount = 0;

                foreach (var line in contentLines)
                {
                    try
                    {
                        count++;

                        var parts = CsvSplitLine(line);

                        var activityId = parts[colId];
                        var activityType = parts[colType];

                        var percentage = 100 * count / lineNumbers;

                        statusUpdater($"#process.activity {percentage}% ({activityId})");

                        var supportedActivityType = _appSettings.ActivityTypes.Contains(activityType);

                        if (long.TryParse(activityId, out var actId) && supportedActivityType)
                        {
                            supportedCount++;

                            var activityFile = parts[colFile];
                            var fileName = Path.GetFileName(activityFile);
                            var orgFile = Path.Combine(tempFolder, activityFile);
                            var destFile = Path.Combine(tempWorkFolder, fileName);

                            supportedActivityCountType++;

                            File.Copy(orgFile, destFile);

                            if (Path.GetExtension(activityFile).ToLower() == ".gz")
                            {
                                destFile = DecompressActivityFile(destFile, tempWorkFolder);
                            }

                            ReadActivity(activityId, destFile, results);

                            TryCleanWorkFolder(tempWorkFolder);
                        }
                    }
                    catch (Exception)
                    {
                        // ignore
                    }
                }

                WriteActivityStreamsToCache(results, supportedCount, statusUpdater);

                return true;
            }

            return false;
        }

        private void ReadActivity(string activityId, string file, Dictionary<string, LineString> results)
        {
            try
            {
                var format = GetFormatForActionFile(file);

                if (format == Formats.GPX)
                {
                    var tracks = GpxCoordinateUtils.CreateLinestringForGpxTrack(file);

                    if (tracks.Count == 1)
                    {
                        results.Add(activityId, tracks[0]);
                    }
                }
                else if (format == Formats.TCX)
                {
                    var tracks = TcxCoordinateUtils.CreateLinestringForTcxTrack(file);

                    if (tracks.Count == 1)
                    {
                        results.Add(activityId, tracks[0]);
                    }
                }
                else if (format == Formats.FIT)
                {
                    var tracks = FitCoordinateUtils.CreateLinestringForForTrack(file);

                    if (tracks.Count == 1)
                    {
                        results.Add(activityId, tracks[0]);
                    }
                }
                else if (format == Formats.JSON)
                {
                    var tracks = JsonCoordinatesUtils.CreateLinestringForForTrack(file);

                    if (tracks.Count == 1)
                    {
                        results.Add(activityId, tracks[0]);
                    }
                }
            }
            catch (Exception)
            {
                // ignore
            }
        }

        private void TryCleanWorkFolder(string workFolder)
        {
            try
            {
                var files = Directory.GetFiles(workFolder);

                foreach (var file in files)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception)
                    {
                        // just try to delete the file, ignore failure
                    }
                }
            }
            catch (Exception)
            {
                // ignore failure
            }
        }

        private List<string> CsvSplitLine(string line)
        {
            var parts = new List<string>();
            var builder = new StringBuilder();

            var inQuote = false;
            var part = string.Empty;

            foreach (var c in line)
            {
                if (c == '"')
                {
                    inQuote = !inQuote;
                }
                else if (c == ',' && !inQuote)
                {
                    parts.Add(builder.ToString());
                    builder = new StringBuilder();
                }
                else
                {
                    builder.Append(c);
                }
            }

            parts.Add(part);

            return parts;
        }

        private Formats GetFormatForActionFile(string file)
        {
            try
            {
                var extentsion = Path.GetExtension(file).ToUpper().Trim('.');
                var supportedFormat = _appSettings.SupportedFormats.Contains(extentsion);

                if (supportedFormat)
                {
                    return (Formats)Enum.Parse(typeof(Formats), extentsion);
                }
            }
            catch (Exception)
            {
                // ignore
            }

            return Formats.UNKNOWN;
        }

        private string DecompressActivityFile(string fileToDecompress, string targetFolder)
        {
            using (var originalFileStream = File.OpenRead(fileToDecompress))
            {
                var folder = Path.GetDirectoryName(fileToDecompress);
                var newFileName = Path.GetFileNameWithoutExtension(fileToDecompress);
                var newFile = Path.Combine(folder, newFileName);

                using (var decompressedFileStream = File.Create(newFile))
                {
                    using (var decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                    }
                }

                return newFile;
            }
        }

        private void WriteActivityStreamsToCache(Dictionary<string, LineString> results, int totalInArchive, ArchiveImportUpdateStatusDelegate statusUpdater)
        {
            var cacheFolder = Path.Combine(_genericSettings.CacheFolder, "Streams", _athleteId);

            if (Directory.Exists(cacheFolder))
            {
                Directory.Delete(cacheFolder, true);
            }

            Directory.CreateDirectory(cacheFolder);

            var resultCount = results.Count;
            var count = 0;
            var successCount = 0;

            foreach (var result in results)
            {
                count++;

                var activityId = result.Key;
                var lineString = result.Value;

                var percentage = 100 * count / resultCount;

                statusUpdater($"#write.to.cache {percentage}% ({activityId})");

                var activityStream = new StravaActivityCoordinateStream();
                activityStream.Latlng = new StravaLatlngStream();
                activityStream.Latlng.Data = new List<List<double>>();

                var first = true;
                var prev = new Coordinate();
                var failed = false;

                foreach (var point in lineString.Coordinates)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        var distanceX = Math.Abs(prev.X - point.X);
                        var distanceY = Math.Abs(prev.Y - point.Y);

                        if (distanceX > _appSettings.MaxDistance || (distanceY > _appSettings.MaxDistance))
                        {
                            failed = true;

                            break;
                        }
                    }

                    prev = point;

                    activityStream.Latlng.Data.Add(new List<double> { point.X, point.Y });
                }

                if (!failed)
                {
                    successCount++;

                    var fileName = Path.Combine(cacheFolder, $"r{activityId}");

                    var json = JsonConvert.SerializeObject(activityStream);

                    File.WriteAllText(fileName, json);
                }
            }

            if (totalInArchive > 0)
            {
                var successPercentage = 100 * successCount / totalInArchive;

                statusUpdater($"#write.to.cache.done {successPercentage}% ({successCount} / {totalInArchive})");
            }
            else
            {
                statusUpdater($"#write.to.cache.done.nothing");
            }


        }
    }
}

