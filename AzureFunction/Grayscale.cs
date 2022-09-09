using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace AzureFunction;

public static class Grayscale
{
    [FunctionName("Grayscale")]
    public static async Task Run(
        [EventGridTrigger] EventGridEvent eventGridEvent,
        [Blob("{data.url}", FileAccess.Read)] Stream inputBlobStream,
        ILogger log)
    {
        log.LogInformation($"Subject: {eventGridEvent.Subject}");

        var parsed = eventGridEvent.TryGetSystemEventData(out var data);

        if (!parsed)
        {
            log.LogError("Message cannot be parsed.");
        }

        if (parsed && data is SubscriptionValidationEventData subscriptionValidationEventData)
        {
            log.LogInformation($"Got SubscriptionValidation event data, " +
                               $"validation code: {subscriptionValidationEventData.ValidationCode}, " +
                               $"topic: {eventGridEvent.Topic}");
            return;
        }

        if (parsed && data is StorageBlobCreatedEventData storageBlobCreatedEventData)
        {
            await ProcessBlobCreationEventAsync(storageBlobCreatedEventData, inputBlobStream, log);
        }
    }

    private static async Task ProcessBlobCreationEventAsync(
        StorageBlobCreatedEventData blobCreatedEventData,
        Stream inputBlobStream,
        ILogger log)
    {
        try
        {
            var createdBlobName = new BlobClient(new Uri(blobCreatedEventData.Url)).Name;

            const string envKey = "AZURE_FUNCTION_STORAGE_CONNECTION";

            var storageConnection = Environment.GetEnvironmentVariable(envKey)
                                    ?? throw new InvalidOperationException(
                                        $"Environment variable is not set: ${envKey}.");

            var outputBlobServiceClient = new BlobServiceClient(storageConnection);

            var outputBlobContainerClient = outputBlobServiceClient
                .GetBlobContainerClient("grayscale");

            using var image = await Image.LoadAsync(inputBlobStream);
            using var outputBlobStream = new MemoryStream();
            image.Mutate(x => x.Grayscale());
            await image.SaveAsPngAsync(outputBlobStream);

            outputBlobStream.Position = 0;

            await outputBlobContainerClient
                .UploadBlobAsync(createdBlobName, outputBlobStream);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "cannot convert");
        }
    }
}