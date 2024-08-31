using LTC2.Shared.Models.Settings;
using Microsoft.AspNetCore.Mvc;

namespace LTC2.Webapps.MainApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly MainClientSettings _mainClientSettings;
        public SettingsController(MainClientSettings mainClientSettings)
        {
            _mainClientSettings = mainClientSettings;
        }

        [HttpGet]
        [Route("mainclientsettings")]
        public MainClientSettings GetMainClientSettings()
        {
            return _mainClientSettings;
        }
    }
}
