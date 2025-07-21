#!/bin/bash

# Default values
EVENT_FORMAT=""

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --format|-f)
            EVENT_FORMAT="$2"
            shift 2
            ;;
        --help|-h)
            echo "Usage: $0 --format [CloudEvent|EventGridEvent]"
            echo "  or: $0 -f [CloudEvent|EventGridEvent]"
            echo ""
            echo "Options:"
            echo "  --format, -f    Event format to send (CloudEvent or EventGridEvent)"
            echo "  --help, -h      Show this help message"
            echo ""
            echo "Examples:"
            echo "  $0 --format CloudEvent"
            echo "  $0 -f EventGridEvent"
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            echo "Use --help or -h for usage information"
            exit 1
            ;;
    esac
done

# Check if required parameter is provided
if [ -z "$EVENT_FORMAT" ]; then
    echo "Error: --format parameter is required"
    echo "Usage: $0 --format [CloudEvent|EventGridEvent]"
    echo "Use --help or -h for more information"
    exit 1
fi

if [ "$EVENT_FORMAT" = "EventGridEvent" ]; then
    echo "Sending EventGridEvent format..."
    curl -X POST \
      "http://localhost:8080/api/EventGridTriggerEmulator" \
      -H "Content-Type: application/json" \
      -d '{
      "id": "99999999-3341-4466-9690-0a03af35228e",
      "topic": "/subscriptions/aaaa0a0a-bb1b-cc2c-dd3d-eeeeee4e4e4e/resourcegroups/acse2e/providers/microsoft.communication/communicationservices/{communication-services-resource-name}",
      "subject": "/phonenumber/15555555555",
      "data": {
        "MessageId": "99999999-3341-4466-9690-0a03af35228e",
        "From": "15555555555",
        "To": "15555555555",
        "Message": "Great to connect with Azure Communication Services events",
        "ReceivedTimestamp": "2020-09-18T00:27:45.32Z"
      },
      "eventType": "Microsoft.Communication.SMSReceived",
      "dataVersion": "1.0",
      "metadataVersion": "1",
      "eventTime": "2020-09-18T00:27:47Z"
    }'

elif [ "$EVENT_FORMAT" = "CloudEvent" ]; then
    echo "Sending CloudEvent format..."
    curl -X POST \
      "http://localhost:7071/api/CloudEventTriggerEmulator" \
      -H "Content-Type: application/cloudevents+json" \
      -d '{
      "specversion": "1.0",
      "type": "Microsoft.Communication.SMSReceived",
      "source": "/subscriptions/aaaa0a0a-bb1b-cc2c-dd3d-eeeeee4e4e4e/resourcegroups/acse2e/providers/microsoft.communication/communicationservices/{communication-services-resource-name}",
      "id": "d29ebbea-3341-4466-9690-0a03af35228e",
      "time": "2020-09-18T00:27:47Z",
      "subject": "/phonenumber/15555555555",
      "datacontenttype": "application/json",
      "data": {
        "MessageId": "d29ebbea-3341-4466-9690-0a03af35228e",
        "From": "15555555555",
        "To": "15555555555",
        "Message": "Great to connect with Azure Communication Services events",
        "ReceivedTimestamp": "2020-09-18T00:27:45.32Z"
      }
    }'

else
    echo "Error: Invalid format '$EVENT_FORMAT'. Use 'CloudEvent' or 'EventGridEvent'"
    echo "Usage: $0 --format [CloudEvent|EventGridEvent]"
    echo "Use --help or -h for more information"
    exit 1
fi