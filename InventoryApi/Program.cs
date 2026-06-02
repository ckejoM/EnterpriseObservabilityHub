using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.Elasticsearch;

var builder = WebApplication.CreateBuilder(args);

// 1. Define the OTel Service Name
var serviceName = "InventoryApi";

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Service", serviceName)
        .WriteTo.Console()
        .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
        {
            IndexFormat = $"applogs-inventoryapi-{DateTime.UtcNow:yyyy-MM}",
            AutoRegisterTemplate = true,
            NumberOfShards = 1,
            NumberOfReplicas = 0
        });
});

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

app.UseSerilogRequestLogging();

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