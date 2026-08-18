using CLWebStore.Catalog.OutboxProcessor.Services;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = FunctionsApplication.CreateBuilder(args);
builder.ConfigureFunctionsWebApplication();

var config = builder.Configuration;

// 1. OpenTelemetry & New Relic Configuration
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        // This is how the service will appear in New Relic
        .AddService("CLWebStore.Catalog.OutboxProcessor"))
    .WithTracing(tracing =>
    {
        tracing
            .AddSource("Microsoft.Azure.Functions.Worker") // Traces function execution
            .AddSource("Azure.*") // Automatically traces Cosmos DB, Service Bus, and Table Storage SDK calls
            .AddHttpClientInstrumentation() // Traces any manual HttpClient calls
            .AddOtlpExporter(options =>
            {
                // Send traces to New Relic via OTLP
                options.Endpoint = new Uri(config["NewRelic:OtlpEndpoint"]!);
                options.Headers = $"api-key={config["NewRelic:LicenseKey"]}";
            });
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddRuntimeInstrumentation() // CPU, GC, Memory metrics
            .AddHttpClientInstrumentation()
            .AddMeter("Microsoft.Azure.Functions.Worker")
            .AddOtlpExporter(options =>
            {
                // Send metrics to New Relic via OTLP
                options.Endpoint = new Uri(config["NewRelic:OtlpEndpoint"]!);
                options.Headers = $"api-key={config["NewRelic:LicenseKey"]}";
            });
    });

// 2. Register Azure Clients
builder.Services.AddAzureClients(clientBuilder =>
{
    // Register Service Bus Client with Hybrid Retry Strategy (Transient)
    clientBuilder.AddServiceBusClient(config["ServiceBusConnectionString"])
        .ConfigureOptions(options =>
        {
            options.RetryOptions = new Azure.Messaging.ServiceBus.ServiceBusRetryOptions
            {
                Mode = Azure.Messaging.ServiceBus.ServiceBusRetryMode.Exponential,
                MaxRetries = 3,
                Delay = TimeSpan.FromSeconds(1),
                MaxDelay = TimeSpan.FromSeconds(5)
            };
        });

    // Register Table Storage Client for the Dead Letter Queue
    clientBuilder.AddTableServiceClient(config["TableStorageConnectionString"]);
});

// 3. Register Application Services
builder.Services.AddSingleton<IDeadLetterStore, DeadLetterStore>();
builder.Services.AddSingleton<IEventPublisher, ServiceBusPublisher>();

builder.Build().Run();
