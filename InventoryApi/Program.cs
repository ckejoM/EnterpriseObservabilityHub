using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// 1. Define the OTel Service Name
var serviceName = "InventoryApi";

// 2. Register OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddOtlpExporter());

// 3. Register Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

app.MapGet("/api/inventory/{productId}", (string productId) =>
{
    // Simulate a random delay to make our traces look realistic
    Thread.Sleep(Random.Shared.Next(10, 100));

    // Simulate a random failure (10% chance) to trigger alerts later
    if (Random.Shared.Next(1, 10) == 1)
    {
        return Results.StatusCode(500);
    }

    return Results.Ok(new { ProductId = productId, InStock = true, Quantity = Random.Shared.Next(1, 50) });
});

app.Run();