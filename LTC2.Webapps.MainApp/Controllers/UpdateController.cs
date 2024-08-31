using LTC2.Shared.Messaging.Generic;
using LTC2.Shared.Messaging.Implementations.FileBasedBroker;
using LTC2.Shared.Messaging.Interfaces;
using LTC2.Shared.Models.Domain;
using LTC2.Webapps.MainApp.Models;
using LTC2.Webapps.MainApp.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;

namespace LTC2.Webapps.MainApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UpdateController : ControllerBase
    {
        private readonly TokenUtils _tokenUtils;
        private readonly IBrokerFactory<Message> _brokerFactory;
        private readonly CalculatorSettings _calculatorSettings;

        public UpdateController(TokenUtils tokenUtils, IBrokerFactory<Message> brokerFactory, CalculatorSettings calculatorSettings)
        {
            _tokenUtils = tokenUtils;
            _brokerFactory = brokerFactory;
            _calculatorSettings = calculatorSettings;
        }

        [HttpPost]
        [Authorize]
        [Route("update")]
        public IActionResult Update([FromQuery] bool refresh, [FromQuery] bool bypassCache = false, [FromQuery] bool isRestore = false, [FromQuery] bool isClear = false)
        {
            var authHeader = _tokenUtils.GetAuthenticationHeader(HttpContext.Request);
            var token = authHeader?.Parameter;

            if (token != null)
            {
                if (_tokenUtils.ValidateToken(token))
                {
                    PostUpdateMessage(_tokenUtils.GetProfileFormToken(token).AthleteId, refresh, bypassCache, isRestore, isClear);

                    return Ok();
                }
            }

            return Unauthorized();
        }

        private void PostUpdateMessage(string athleteIdAsString, bool refresh, bool bypassCache, bool isRestore, bool isClear)
        {
            var broker = _brokerFactory.CreateBroker();
            var connection = broker.Connect(_calculatorSettings.BrokerConnection);

            var target = new Target()
            {
                Name = "INPUT"
            };

            var producer = connection.CreateProducer(target);
            var message = broker.CreateEmptyMessage(MessageType.Text);

            var calculationJob = new CalculationJob()
            {
                AthleteId = Convert.ToInt64(athleteIdAsString),
                Refresh = refresh,
                BypassCache = bypassCache,
                IsRestoreInterMediate = isRestore,
                IsClearInterMediate = isClear
            };

            var payLoad = JsonConvert.SerializeObject(calculationJob);

            message.Group = $"s{athleteIdAsString}";
            message.Id = $"{DateTime.UtcNow:yyyyMMddHHmmssFFF}{message.Group}";
            message.Payload = payLoad;
            message.Priority = refresh ? MessagePriority.MediumHigh : MessagePriority.High;

            producer.Produce(message);
        }
    }
}
