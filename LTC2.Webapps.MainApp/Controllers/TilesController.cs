using LTC2.Shared.SpatiaLiteRepository.Repositories;
using LTC2.Webapps.MainApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace LTC2.Webapps.MainApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TilesController : ControllerBase
    {
        private readonly AppSettings _appSettings;
        private readonly TilesRepository _tilesRepository;

        public TilesController(AppSettings appSettings, TilesRepository tilesRepository)
        {
            _appSettings = appSettings;
            _tilesRepository = tilesRepository;
        }

        [HttpGet]
        [Route("tilef/{z}/{x}/{y}.pbf")]
        [ResponseCache(VaryByHeader = "User-Agent", Duration = 1800)]
        public IActionResult GetTileF([FromRoute] int z, [FromRoute] int x, [FromRoute] int y)
        {
            var fileName = Path.Combine(_appSettings.TilesFolder, $"{z}\\{x}\\", $"{y}.pbf");

            if (System.IO.File.Exists(fileName))
            {
                for (var retry = 0; retry < 3; retry++)
                {
                    try
                    {
                        var stream = new FileStream(fileName, FileMode.Open);

                        return File(stream, "application/x-protobuf", Path.GetFileName(stream.Name));
                    }
                    catch { }
                }
            }

            return NotFound();
        }

        [HttpGet]
        [Route("tile/{z}/{x}/{y}.pbf")]
        [ResponseCache(VaryByHeader = "User-Agent", Duration = 1800)]
        public IActionResult GetTile([FromRoute] int z, [FromRoute] int x, [FromRoute] int y)
        {
            try
            {
                var stream = _tilesRepository.GetTileStream(z, x, y);

                if (stream != null)
                {
                    return File(stream, "application/x-protobuf");
                }
            }
            catch { }

            return NotFound();
        }
    }
}
