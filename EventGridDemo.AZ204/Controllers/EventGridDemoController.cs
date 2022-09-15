using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EventGridDemo.AZ204.Controllers;

[ApiController]
[Route("[controller]")]
public class EventGridDemoController : ControllerBase
{
    private bool EventTypeSubscriptionValidation
        => HttpContext.Request.Headers["aeg-event-type"].FirstOrDefault() ==
           "SubscriptionValidation";

    private bool EventTypeNotification
        => HttpContext.Request.Headers["aeg-event-type"].FirstOrDefault() ==
           "Notification";

    private readonly ILogger<EventGridDemoController> _logger;

    public EventGridDemoController(ILogger<EventGridDemoController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Post()
    {
        _logger.LogInformation($"Web hook received: {Request.Method}");
        System.Diagnostics.Trace.TraceInformation($"Web hook received: {Request.Method}");

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
                    var info =
                        $"Subscription validation event with " +
                        $"code: {subValidationEventData.ValidationCode}, " +
                        $"url: {subValidationEventData.ValidationUrl}";

                    System.Diagnostics.Trace.TraceInformation(info);

                    _logger.LogInformation(info);

                    var response = new SubscriptionValidationResponse();

                    return new ObjectResult(response);
                }
            }

            if (EventTypeNotification)
            {
                var notificationEvent = events.First();

                System.Diagnostics.Trace.TraceInformation($"Notification received: {notificationEvent.Subject}");
                _logger.LogInformation($"Notification received: {notificationEvent.Subject}");

                return new OkResult();
            }
        }

        return new OkResult();
    }
}