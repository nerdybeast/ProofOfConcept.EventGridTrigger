// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}

using Azure.Messaging.EventGrid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ProofOfConcept.EventGridTrigger;

public class EventGridTriggerEmulator(ILogger<EventGridTriggerEmulator> logger)
{
    private readonly ILogger<EventGridTriggerEmulator> _logger = logger;

    [Function(nameof(EventGridTriggerEmulator))]
    [QueueOutput("eventgrid-output-queue")]
    public EventGridEvent Run([EventGridTrigger] EventGridEvent eventGridEvent)
    {
        _logger.LogInformation("Event type: {EventType}, Event subject: {Subject}", eventGridEvent.EventType, eventGridEvent.Subject);
        return eventGridEvent;
    }
}