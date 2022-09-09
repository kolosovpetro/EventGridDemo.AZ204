using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EventHubDemo.AZ204.Controllers;

[ApiController]
[Route("[controller]")]
public class EvenHubDemoController : ControllerBase
{
    private bool EventTypeSubscriptionValidation
        => HttpContext.Request.Headers["aeg-event-type"].FirstOrDefault() ==
           "SubscriptionValidation";

    private bool EventTypeNotification
        => HttpContext.Request.Headers["aeg-event-type"].FirstOrDefault() ==
           "Notification";

    private readonly ILogger<EvenHubDemoController> _logger;

    public EvenHubDemoController(ILogger<EvenHubDemoController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Post()
    {
        using (var requestStream = new StreamReader(Request.Body))
        {
            var bodyJson = await requestStream.ReadToEndAsync();

            var events = JsonConvert.DeserializeObject<List<EventGridEvent>>(bodyJson);

            if (EventTypeSubscriptionValidation)
            {
                var currentEvent = events.First();

                var parsed = currentEvent.TryGetSystemEventData(out var data);

                if (parsed && data is SubscriptionValidationEventData subValidationEventData)
                {
                    var response = new { subValidationEventData };

                    return new ObjectResult(response);
                }
            }

            if (EventTypeNotification)
            {
                var notificationEvent = events.First();

                _logger.LogInformation(notificationEvent.Subject);

                return new OkResult();
            }
        }

        return new BadRequestResult();
    }
}