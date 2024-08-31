namespace LTC2.Shared.Secrets.Interfaces
{
    public interface ISecretsVault
    {
        public string GetSecret(string type, string id, bool temporary);

        public void StoreSecret(string type, string id, string secret, bool temporary);

        public bool SecretExists(string type, string id, bool temporary);

        public List<string> GetSecrects(string type);

        public void RemoveSecrect(string type, string id);

        public void RemoveAllTempSecrets(string type);
    }
}
