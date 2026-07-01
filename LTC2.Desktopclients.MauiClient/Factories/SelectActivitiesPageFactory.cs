using LTC2.Desktopclients.MauiClient.Interfaces;
using LTC2.Desktopclients.MauiClient.Models;
using LTC2.Desktopclients.MauiClient.Pages;
using LTC2.Desktopclients.MauiClient.Services;
using LTC2.Shared.BaseMessages.Interfaces;

namespace LTC2.Desktopclients.MauiClient.Factories
{
    public class SelectActivitiesPageFactory : ISelectActivitiesPageFactory
    {
        private readonly IBaseTranslationService _translationService;
        private readonly MultiSportManager _multiSportManager;
        private readonly AppSettings _appSettings;

        public SelectActivitiesPageFactory(
            IBaseTranslationService translationService,
            MultiSportManager multiSportManager,
            AppSettings appSettings)
        {
            _translationService = translationService;
            _multiSportManager = multiSportManager;
            _appSettings = appSettings;
        }

        public SelectActivitiesPage Create()
        {
            return new SelectActivitiesPage(_translationService, _multiSportManager);
        }
    }
}
