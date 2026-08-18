using CLWebStore.Catalog.ReadModelProjector.Configuration;
using CLWebStore.Catalog.ReadModelProjector.Dispatching;
using CLWebStore.Catalog.ReadModelProjector.EventHandling;
using CLWebStore.Catalog.ReadModelProjector.Observability;
using CLWebStore.Catalog.ReadModelProjector.Projection;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Text.Json;

var builder = FunctionsApplication.CreateBuilder(args);
var configuration = builder.Configuration;

var newRelicEndpoint = GetRequiredUri(configuration, "NewRelic:OtlpEndpoint");
var newRelicLicenseKey = GetRequiredConfigurationValue(configuration, "NewRelic:LicenseKey");

builder.Services
    .AddOptions<ProjectorOptions>()
    .Bind(configuration)
    .Validate(options => !string.IsNullOrWhiteSpace(options.ServiceBusConnection), "ServiceBusConnection is required.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.ServiceBusSubscriptionName),
        "ServiceBusSubscriptionName is required.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.PostgresConnectionString),
        "PostgresConnectionString is required.")
    .ValidateOnStart();

builder.Services
    .AddOptions<NewRelicOptions>()
    .Bind(configuration.GetSection(NewRelicOptions.SectionName))
    .Validate(options => Uri.TryCreate(options.OtlpEndpoint, UriKind.Absolute, out _), "New Relic OTLP endpoint is invalid.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.LicenseKey), "New Relic license key is required.")
    .ValidateOnStart();

builder.Services.AddSingleton(new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true
});

builder.Services.AddSingleton(ReadModelProjectorDiagnostics.ActivitySource);
builder.Services.AddSingleton(sp =>
{
    var connectionString = GetRequiredConfigurationValue(configuration, "PostgresConnectionString");
    return new NpgsqlDataSourceBuilder(connectionString).Build();
});

builder.Services.AddSingleton<IEventDispatcher, EventDispatcher>();
builder.Services.AddSingleton<IProductEventHandler, ProductCreatedEventHandler>();
builder.Services.AddSingleton<IProductEventHandler, ProductUpdatedEventHandler>();
builder.Services.AddSingleton<IProductProjectionService, ProductProjectionService>();

var resourceBuilder = ResourceBuilder
    .CreateDefault()
    .AddService(ReadModelProjectorDiagnostics.ServiceName);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(ReadModelProjectorDiagnostics.ServiceName))
    .WithTracing(tracing =>
    {
        tracing
            .AddSource(ReadModelProjectorDiagnostics.ActivitySourceName)
            .AddSource("Microsoft.Azure.Functions.Worker")
            .AddSource("Azure.*")
            .AddOtlpExporter(options =>
            {
                options.Endpoint = newRelicEndpoint;
                options.Protocol = OtlpExportProtocol.Grpc;
                options.Headers = $"api-key={newRelicLicenseKey}";
            });
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddRuntimeInstrumentation()
            .AddProcessInstrumentation()
            .AddMeter("Microsoft.Azure.Functions.Worker")
            .AddOtlpExporter(options =>
            {
                options.Endpoint = newRelicEndpoint;
                options.Protocol = OtlpExportProtocol.Grpc;
                options.Headers = $"api-key={newRelicLicenseKey}";
            });
    });

builder.Logging.AddOpenTelemetry(options =>
{
    options.SetResourceBuilder(resourceBuilder);
    options.IncludeFormattedMessage = true;
    options.IncludeScopes = true;
    options.AddOtlpExporter(exporterOptions =>
    {
        exporterOptions.Endpoint = newRelicEndpoint;
        exporterOptions.Protocol = OtlpExportProtocol.Grpc;
        exporterOptions.Headers = $"api-key={newRelicLicenseKey}";
    });
});

builder.Build().Run();

static string GetRequiredConfigurationValue(IConfiguration configuration, string key)
{
    var value = configuration[key];

    if (string.IsNullOrWhiteSpace(value))
    {
        throw new InvalidOperationException($"{key} is required.");
    }

    return value;
}

static Uri GetRequiredUri(IConfiguration configuration, string key)
{
    var value = GetRequiredConfigurationValue(configuration, key);

    if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
    {
        throw new InvalidOperationException($"{key} must be an absolute URI.");
    }

    return uri;
}
