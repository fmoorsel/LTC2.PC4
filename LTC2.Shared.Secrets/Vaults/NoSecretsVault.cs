using LTC2.Shared.Models.Settings;
using LTC2.Shared.Secrets.Interfaces;

namespace LTC2.Shared.Secrets.Vaults
{
    public class NoSecretsVault: ISecretsVault
    {
        private readonly GenericSettings _genericSettings;

        public NoSecretsVault(GenericSettings genericSettings)
        {
            _genericSettings = genericSettings;
        }
        public string GetSecret(string type, string id, bool temporary)
        {
            if (SecretExists(type, id, temporary))
            {
                var fileName = GetFileName(type, id, temporary);

                return GetSecret(fileName);
            }

            return string.Empty;
        }

        public void StoreSecret(string type, string id, string secret, bool temporary)
        {
            if (!Directory.Exists(_genericSettings.SecretsFolder))
            {
                Directory.CreateDirectory(_genericSettings.SecretsFolder);
            }

            var fileName = Path.GetFullPath(GetFileName(type, id, temporary));
            
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            File.WriteAllText(fileName, secret);
        }

        public bool SecretExists(string type, string id, bool temp)
        {
            if (_genericSettings.SecretsFolder != null)
            {
                return File.Exists(GetFileName(type, id, temp));
            }

            return false;
        }

        public List<string> GetSecrects(string type)
        {
            if (_genericSettings.SecretsFolder != null)
            {
                if (!Directory.Exists(_genericSettings.SecretsFolder))
                {
                    Directory.CreateDirectory(_genericSettings.SecretsFolder);
                    
                    return new List<string>();
                }
                
                var result = new List<string>();
                var extension = "dat";

                var folder = _genericSettings.SecretsFolder;
                var secretFilesSearhPath = $"s-{type}-*.{extension}";

                var secretFiles = Directory.GetFiles(folder, secretFilesSearhPath);

                foreach (var fileName in secretFiles)
                {
                    var entry = GetSecret(fileName);

                    result.Add(entry);
                }

                return result;
            }

            throw new FileNotFoundException("Secrets folder not set");
        }

        public void RemoveSecrect(string type, string id)
        {
            if (_genericSettings.SecretsFolder != null)
            {
                var fileName = GetFileName(type, id, false);

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                return;
            }

            throw new FileNotFoundException("Secrets folder not set");
        }

        public void RemoveAllTempSecrets(string type)
        {
            if (_genericSettings.SecretsFolder != null)
            {
                var result = new List<string>();
                var extension = "$$$";

                var folder = _genericSettings.SecretsFolder;
                var secretFilesSearhPath = $"s-{type}-*.{extension}";

                var secretFiles = Directory.GetFiles(folder, secretFilesSearhPath);

                foreach (var fileName in secretFiles)
                {
                    File.Delete(fileName);
                }

                return;
            }

            throw new FileNotFoundException("Secrets folder not set");
        }
        
        private string GetFileName(string type, string profile, bool temp)
        {
            if (_genericSettings.SecretsFolder != null)
            {
                var extension = temp ? "$$$" : "dat";

                var folder = _genericSettings.SecretsFolder;
                var secretFile = Path.Combine(folder, $"s-{type}-{profile}.{extension}");

                return secretFile;
            }

            throw new FileNotFoundException("Secrets folder not set");
        }
        
        private string GetSecret(string fileName)
        {
            return File.ReadAllText(fileName);
        }
    }    
}

