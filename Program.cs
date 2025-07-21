using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Add health checks
builder.Services.AddHealthChecks();
    // .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Application is running"))
    // .AddCheck("database", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Database connection is available"))
    // .AddCheck("storage", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Storage is available"));

await builder.Build().RunAsync();
