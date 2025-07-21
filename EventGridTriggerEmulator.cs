// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}

using Azure.Messaging;
using Azure.Messaging.EventGrid;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;

namespace ProofOfConcept.EventGridTrigger;

public class EventGridTriggerEmulator(ILogger<EventGridTriggerEmulator> logger)
{
    private readonly ILogger<EventGridTriggerEmulator> _logger = logger;
    private static readonly ActivitySource ActivitySource = new("EventGridTriggerEmulator.EventGrid");
    private const string ErrorTag = "error";

    [Function("EventGridTriggerEmulator")]
    [QueueOutput("eventgrid-output-queue")]
    public async Task<EventGridEvent?> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        using var activity = ActivitySource.StartActivity("EventGrid.ProcessEvent");
        
        _logger.LogInformation("EventGrid HTTP trigger function processed a request.");

        // Read the request body
        var requestBody = await req.ReadAsStringAsync();

        if (string.IsNullOrEmpty(requestBody))
        {
            activity?.SetTag(ErrorTag, "empty_request_body");
            activity?.SetTag("error", "empty_request_body");
            return null;
        }

        // Parse EventGrid events
        var eventGridEvent = EventGridEvent.Parse(BinaryData.FromString(requestBody));

        if (eventGridEvent == null)
        {
            activity?.SetTag(ErrorTag, "parse_failed");
            activity?.SetTag("error", "parse_failed");
            return null;
        }

        activity?.SetTag("event.type", eventGridEvent.EventType);
        activity?.SetTag("event.subject", eventGridEvent.Subject);
        activity?.SetTag("event.id", eventGridEvent.Id);

        _logger.LogInformation("Event type: {EventType}, Event subject: {Subject}", eventGridEvent.EventType, eventGridEvent.Subject);

        return eventGridEvent;
    }

    [Function("CloudEventTriggerEmulator")]
    [QueueOutput("cloudevent-output-queue")]
    public async Task<CloudEvent?> RunCloudEvent([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        using var activity = ActivitySource.StartActivity("CloudEvent.ProcessEvent");
        
        _logger.LogInformation("CloudEvent HTTP trigger function processed a request.");

        // Read the request body
        var requestBody = await req.ReadAsStringAsync();
        
        if (string.IsNullOrEmpty(requestBody))
        {
            _logger.LogWarning("Request body is empty.");
            activity?.SetTag("error", "empty_request_body");
            return null;
        }
        
        // Parse CloudEvent
        var cloudEvent = CloudEvent.Parse(BinaryData.FromString(requestBody));

        if (cloudEvent == null)
        {
            _logger.LogWarning("Failed to parse CloudEvent from request body.");
            activity?.SetTag("error", "parse_failed");
            return null;
        }

        activity?.SetTag("cloudevent.type", cloudEvent.Type);
        activity?.SetTag("cloudevent.subject", cloudEvent.Subject);
        activity?.SetTag("cloudevent.source", cloudEvent.Source);
        activity?.SetTag("cloudevent.id", cloudEvent.Id);

        _logger.LogInformation("Cloud Event - Type: {Type}, Subject: {Subject}, Source: {Source}", cloudEvent.Type, cloudEvent.Subject, cloudEvent.Source);

        return cloudEvent;
    }
}