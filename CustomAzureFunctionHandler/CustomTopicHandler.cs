using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;

namespace CustomAzureFunctionHandler;

public static class CustomTopicHandler
{
    [FunctionName("CustomTopicHandler")]
    public static async Task Run(
        [EventGridTrigger] EventGridEvent eventGridEvent,
        ILogger log)
    {
        log.LogInformation($"Type: {eventGridEvent.EventType}");
        log.LogInformation($"Subject: {eventGridEvent.Subject}");
        log.LogInformation($"Topic: {eventGridEvent.Topic}");
        log.LogInformation($"EventTime: {eventGridEvent.EventTime}");
    }
}