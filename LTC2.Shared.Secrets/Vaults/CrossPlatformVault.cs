using LTC2.Shared.Models.Settings;
using LTC2.Shared.Secrets.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace LTC2.Shared.Secrets.Vaults;

public class CrossPlatformVault(GenericSettings genericSettings) : ISecretsVault
{
    private readonly GenericSettings _genericSettings = genericSettings;

    private bool _isSetup = false;
    private IDataProtectionProvider? _provider;

    private readonly Lock _setupLock = new Lock();

    public string GetSecret(string type, string id, bool temp)
    {
        if (!SecretExists(type, id, temp))
        {
            return string.Empty;
        }

        var fileName = GetFileName(type, id, temp);

        return GetSecret(fileName);
    }

    public void StoreSecret(string type, string id, string secret, bool temp)
    {
        if (!Directory.Exists(_genericSettings.SecretsFolder))
        {
            Directory.CreateDirectory(_genericSettings.SecretsFolder);
        }

        var fileName = Path.GetFullPath(GetFileName(type, id, temp));

        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }

        EncryptDataToStream(secret, fileName);
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
            var secretFilesSearchPath = $"s-{type}-*.{extension}";

            var secretFiles = Directory.GetFiles(folder, secretFilesSearchPath);

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
            var secretFilesSearchPath = $"s-{type}-*.{extension}";

            var secretFiles = Directory.GetFiles(folder, secretFilesSearchPath);

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
        var decrypted = DecryptDataFromStream(fileName);

        return decrypted;
    }

    private void EncryptDataToStream(string secret, string fileName)
    {
        SetupDataProtection();

        var protector = _provider?.CreateProtector(fileName);

        if (protector == null)
        {
            throw new InvalidOperationException("Data protection provider is not initialized");
        }

        var toEncrypt = Encoding.ASCII.GetBytes(secret);

        var encryptedData = protector.Protect(toEncrypt);

        File.WriteAllBytes(fileName, encryptedData);
    }

    private string DecryptDataFromStream(string fileName)
    {
        SetupDataProtection();

        var protector = _provider?.CreateProtector(fileName);

        if (protector == null)
        {
            throw new InvalidOperationException("Data protection provider is not initialized");
        }

        var buffer = File.ReadAllBytes(fileName);
        var outBuffer = protector.Unprotect(buffer);

        return Encoding.ASCII.GetString(outBuffer);
    }

    private void SetupDataProtection()
    {
        lock (_setupLock)
        {
            if (_isSetup)
            {
                return;
            }

            var secretsFolder = _genericSettings.SecretsFolder;

            if (secretsFolder == null)
            {
                throw new FileNotFoundException("Secrets folder is not set");
            }

            var certificate = SetupDataProtectionCertificate();

            var path = Path.Combine(secretsFolder, "ltc2DataProtection");
            var dirInfo = new DirectoryInfo(path);
            var provider = DataProtectionProvider.Create(dirInfo, certificate);

            _provider = provider;

            _isSetup = true;
        }
    }

    private static X509Certificate2 CreateSelfSignedDataProtectionCertificate(string subjectName)
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(subjectName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddYears(15));

        return certificate;
    }

    private static void InstallCertificateAsNonExportable(X509Certificate2 certificate)
    {
        var rawData = certificate.Export(X509ContentType.Pkcs12, password: null as string);

        using var loadedCert = X509CertificateLoader.LoadPkcs12(
            rawData,
            password: null,
            keyStorageFlags: X509KeyStorageFlags.PersistKeySet);

        using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser, OpenFlags.ReadWrite);
        store.Add(loadedCert);
    }


    private static X509Certificate2 SetupDataProtectionCertificate()
    {
        var prefix = "CN=LTC2-SECRETS-VAULT-DataProtection-Certificate";
        var subjectName = $"{prefix}-{DateTime.UtcNow.Ticks}";
        using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser, OpenFlags.ReadOnly);
        var certs = store.Certificates.Where(c => c.Subject.StartsWith(prefix)).ToList();

        if (certs.Count > 0)
        {
            var oldestCertSubject = certs.Min(c => c.Subject);
            var cert = certs.FirstOrDefault(c => c.Subject == oldestCertSubject);

            if (cert != null)
            {
                return cert;
            }
        }

        var certificate = CreateSelfSignedDataProtectionCertificate(subjectName);
        InstallCertificateAsNonExportable(certificate);

        return certificate;
    }

}