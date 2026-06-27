using LTC2.Desktopclients.AvaloniaClient.Interfaces;
using LTC2.Desktopclients.AvaloniaClient.Models;
using LTC2.Desktopclients.AvaloniaClient.Services;
using LTC2.Desktopclients.AvaloniaClient.Windows;
using LTC2.Shared.BaseMessages.Interfaces;

namespace LTC2.Desktopclients.AvaloniaClient.Factories
{
    public class SelectActivitiesWindowFactory : ISelectActivitiesWindowFactory
    {
        private readonly IBaseTranslationService _translationService;
        private readonly MultiSportManager _multiSportManager;
        private readonly AppSettings _appSettings;

        public SelectActivitiesWindowFactory(
            IBaseTranslationService translationService,
            MultiSportManager multiSportManager,
            AppSettings appSettings)
        {
            _translationService = translationService;
            _multiSportManager = multiSportManager;
            _appSettings = appSettings;
        }

        public SelectActivitiesWindow Create()
        {
            return new SelectActivitiesWindow(_translationService, _multiSportManager, _appSettings);
        }
    }
}
