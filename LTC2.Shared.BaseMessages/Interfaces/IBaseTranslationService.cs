using System.Collections.Generic;

namespace LTC2.Shared.BaseMessages.Interfaces
{
    public interface IBaseTranslationService
    {
        public string CurrentLanguage { get; }

        public void LoadMessagesForLanguage(string language);

        public string GetMessage(string message);

        public string GetMessage(string message, List<string> parameters);

        public string GetMessage(string message, string parameter);

    }
}
