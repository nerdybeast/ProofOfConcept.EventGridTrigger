using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Add health checks
builder.Services.AddHealthChecks();
// .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Application is running"))
// .AddCheck("database", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Database connection is available"))
// .AddCheck("storage", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Storage is available"));

// Add OpenTelemetry
ResourceBuilder resource = ResourceBuilder.CreateDefault()
	.AddService("EventGridTriggerEmulator", "1.0.0");

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .SetResourceBuilder(resource)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource("EventGridTriggerEmulator.EventGrid")
            .AddSource("EventGridTriggerEmulator.HealthChecks")
            .AddConsoleExporter();
            // .AddOtlpExporter(); // Uncomment to send to OTLP endpoint like Jaeger, Zipkin, etc.
    })
    .WithMetrics(metrics =>
    {
        metrics
            .SetResourceBuilder(resource)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter();
            // .AddOtlpExporter(); // Uncomment to send to OTLP endpoint
    });

await builder.Build().RunAsync();
