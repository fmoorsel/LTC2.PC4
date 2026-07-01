using LTC2.Desktopclients.MauiClient.Interfaces;
using LTC2.Desktopclients.MauiClient.Pages;
using LTC2.Desktopclients.MauiClient.Services;
using LTC2.Shared.BaseMessages.Interfaces;

namespace LTC2.Desktopclients.MauiClient.Factories
{
    public class SelectProfilePageFactory : ISelectProfilePageFactory
    {
        private readonly ProfileManager _profileManager;
        private readonly IBaseTranslationService _translationService;
        private readonly ProfileManagerStarter _profileManagerStarter;

        public SelectProfilePageFactory(
            ProfileManager profileManager,
            IBaseTranslationService translationService,
            ProfileManagerStarter profileManagerStarter)
        {
            _profileManager = profileManager;
            _translationService = translationService;
            _profileManagerStarter = profileManagerStarter;
        }

        public SelectProfilePage Create()
        {
            return new SelectProfilePage(_profileManager, _translationService, _profileManagerStarter);
        }
    }
}
