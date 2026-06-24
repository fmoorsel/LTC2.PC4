using LTC2.Desktopclients.AvaloniaClient.Interfaces;
using LTC2.Desktopclients.AvaloniaClient.Services;
using LTC2.Desktopclients.AvaloniaClient.Windows;
using LTC2.Shared.BaseMessages.Interfaces;

namespace LTC2.Desktopclients.AvaloniaClient.Factories
{
    public class SelectProfileWindowFactory : ISelectProfileWindowFactory
    {
        private readonly ProfileManager _profileManager;
        private readonly IBaseTranslationService _translationService;
        private readonly ProfileManagerStarter _profileManagerStarter;

        public SelectProfileWindowFactory(
            ProfileManager profileManager,
            IBaseTranslationService translationService,
            ProfileManagerStarter profileManagerStarter)
        {
            _profileManager = profileManager;
            _translationService = translationService;
            _profileManagerStarter = profileManagerStarter;
        }

        public SelectProfileWindow Create()
        {
            return new SelectProfileWindow(_profileManager, _translationService, _profileManagerStarter);
        }
    }
}
