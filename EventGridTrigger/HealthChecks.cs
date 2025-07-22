using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;

namespace ProofOfConcept.EventGridTrigger;

public class HealthChecks(ILogger<HealthChecks> logger, HealthCheckService healthCheckService)
{
    private readonly ILogger<HealthChecks> _logger = logger;
    private readonly HealthCheckService _healthCheckService = healthCheckService;
    private static readonly ActivitySource ActivitySource = new("EventGridTriggerEmulator.HealthChecks");

	// Path is at ".../api/readiness"
	[Function("Readiness")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "readiness")] HttpRequestData req)
    {
        using var activity = ActivitySource.StartActivity("HealthCheck.Readiness");
        
        _logger.LogInformation("Readiness check requested.");
        
        activity?.SetTag("health.check.type", "readiness");
        activity?.SetTag("service.name", "EventGridTriggerEmulator");

		// Run health checks
		HealthReport healthCheckResult = await _healthCheckService.CheckHealthAsync();

		bool isHealthy = healthCheckResult.Status == HealthStatus.Healthy;
		HttpStatusCode statusCode = isHealthy ? HttpStatusCode.OK : HttpStatusCode.ServiceUnavailable;

        activity?.SetTag("health.check.status", healthCheckResult.Status.ToString());
        activity?.SetTag("health.check.duration_ms", healthCheckResult.TotalDuration.TotalMilliseconds);

		HttpResponseData response = req.CreateResponse(statusCode);

        var healthStatus = new
        {
            status = healthCheckResult.Status.ToString().ToLower(),
            timestamp = DateTime.UtcNow,
            service = "EventGridTriggerEmulator",
            totalDuration = healthCheckResult.TotalDuration.TotalMilliseconds,
            checks = healthCheckResult.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString().ToLower(),
                duration = entry.Value.Duration.TotalMilliseconds,
                description = entry.Value.Description,
                exception = entry.Value.Exception?.Message
            })
        };

        await response.WriteAsJsonAsync(healthStatus);

        _logger.LogInformation("Readiness check completed - service status: {Status}", healthCheckResult.Status);
        
        return response;
    }
}
