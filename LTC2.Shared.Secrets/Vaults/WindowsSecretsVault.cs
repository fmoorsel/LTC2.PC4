using LTC2.Shared.Models.Settings;
using LTC2.Shared.Secrets.Interfaces;
using System.Security.Cryptography;
using System.Text;


namespace LTC2.Shared.Secrets.Vaults
{
    public class WindowsSecretsVault : ISecretsVault
    {
        private readonly GenericSettings _genericSettings;

        public WindowsSecretsVault(GenericSettings genericSettings)
        {
            _genericSettings = genericSettings;
        }

        public string GetSecret(string type, string id, bool temp)
        {
            if (SecretExists(type, id, temp))
            {
                var fileName = GetFileName(type, id, temp);

                return GetSecret(fileName);
            }

            return string.Empty;
        }

        public bool SecretExists(string type, string id, bool temp)
        {
            if (_genericSettings.SecretsFolder != null)
            {
                return File.Exists(GetFileName(type, id, temp));
            }

            return false;

        }

        public void StoreSecret(string type, string id, string secret, bool temp)
        {
            if (!Directory.Exists(_genericSettings.SecretsFolder))
            {
                Directory.CreateDirectory(_genericSettings.SecretsFolder);
            }

            var fileName = Path.GetFullPath(GetFileName(type, id, temp));

            var toEncrypt = UnicodeEncoding.ASCII.GetBytes(secret);
            var entropy = UnicodeEncoding.ASCII.GetBytes(fileName);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            EncryptDataToStream(toEncrypt, entropy, DataProtectionScope.LocalMachine, fileName);
        }

        public List<string> GetSecrects(string type)
        {
            if (_genericSettings.SecretsFolder != null)
            {
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

        private string GetSecret(string fileName)
        {
            var entropy = UnicodeEncoding.ASCII.GetBytes(Path.GetFullPath(fileName));

            var decrypted = DecryptDataFromStream(entropy, DataProtectionScope.LocalMachine, fileName);

            return UnicodeEncoding.ASCII.GetString(decrypted);
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

        private void EncryptDataToStream(byte[] buffer, byte[] entropy, DataProtectionScope scope, string fileName)
        {

            var encryptedData = ProtectedData.Protect(buffer, entropy, scope);

            File.WriteAllBytes(fileName, encryptedData);
        }

        private byte[] DecryptDataFromStream(byte[] entropy, DataProtectionScope scope, string fileName)
        {
            var buffer = File.ReadAllBytes(fileName);
            var outBuffer = ProtectedData.Unprotect(buffer, entropy, scope);

            return outBuffer;
        }
    }
}
