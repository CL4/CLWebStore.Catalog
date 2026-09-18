using Asp.Versioning;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using CLWebStore.Catalog.API;
using CLWebStore.Catalog.API.Middleware;
using CLWebStore.Catalog.Application.DependencyInjection;
using CLWebStore.Catalog.Infrastructure.DependencyInjection;
using CLWebStore.Catalog.Infrastructure.Persistence.Cosmos;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// --- 0. INJECT AZURE KEY VAULT INTO ICONFIGURATION ---
var vaultUriString = builder.Configuration["KeyVault:VaultUri"];

if (!string.IsNullOrWhiteSpace(vaultUriString))
{
    var vaultUri = new Uri(vaultUriString);
    SecretClient secretClient;

    if (builder.Environment.IsDevelopment())
    {
        // 1. Emulator Bypass Options
        var clientOptions = new SecretClientOptions { DisableChallengeResourceVerification = true };

        // 2. Use the Fake Token Generator instead of Visual Studio!
        secretClient = new SecretClient(vaultUri, new DummyTokenCredential(), clientOptions);
    }
    else
    {
        // Production strictly uses the real Azure AD credentials
        secretClient = new SecretClient(vaultUri, new DefaultAzureCredential());
    }

    builder.Configuration.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
}
// -----------------------------------------------------

// 1. Add Layer Registrations (from existing setup)
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// 2. Controllers & API Versioning
builder.Services.AddControllers();
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddMvc().AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// 3. Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 4. AutoMapper
var autoMapperLicenseKey = builder.Configuration["AutoMapper:LicenseKey"];
builder.Services.AddAutoMapper(cfg =>
{
    cfg.LicenseKey = autoMapperLicenseKey;
},
// Scan the API Layer for profiles
typeof(Program).Assembly,
// Scan the Application Layer for profiles
typeof(ApplicationServiceRegistration).Assembly);

// 5. Global Exception Handler (.NET 8)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// 6. Observability (OpenTelemetry / optional New Relic export)
var serviceName = "CLWebStore.Catalog";
var resourceBuilder = ResourceBuilder.CreateDefault().AddService(serviceName);

var enableNewRelicExport =
    builder.Configuration.GetValue<bool>("Observability:EnableNewRelicExport");

Uri? newRelicEndpoint = null;
string? newRelicApiKey = null;

if (enableNewRelicExport)
{
    var newRelicEndpointString =
        builder.Configuration["Observability:NewRelicOtlpEndpoint"];

    if (string.IsNullOrWhiteSpace(newRelicEndpointString))
    {
        throw new InvalidOperationException(
            "Observability:NewRelicOtlpEndpoint is required when " +
            "Observability:EnableNewRelicExport is true.");
    }

    newRelicEndpoint = new Uri(newRelicEndpointString);

    newRelicApiKey =
        builder.Configuration["Observability:NewRelicApiKey"];
}

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .SetResourceBuilder(resourceBuilder)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource("CLWebStore.Catalog.*");

        if (enableNewRelicExport)
        {
            tracing.AddOtlpExporter(options =>
            {
                options.Endpoint = newRelicEndpoint!;
                options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                options.Headers = $"api-key={newRelicApiKey}";
            });
        }
    })
    .WithMetrics(metrics =>
    {
        metrics
            .SetResourceBuilder(resourceBuilder)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation();

        if (enableNewRelicExport)
        {
            metrics.AddOtlpExporter(options =>
            {
                options.Endpoint = newRelicEndpoint!;
                options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                options.Headers = $"api-key={newRelicApiKey}";
            });
        }
    });

// 7. Configure logging telemetry export
if (enableNewRelicExport)
{
    builder.Logging.AddOpenTelemetry(options =>
    {
        options.SetResourceBuilder(resourceBuilder);
        options.IncludeFormattedMessage = true;
        options.IncludeScopes = true;

        options.AddOtlpExporter(exporterOptions =>
        {
            exporterOptions.Endpoint = newRelicEndpoint!;
            exporterOptions.Protocol =
                OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;

            exporterOptions.Headers =
                $"api-key={newRelicApiKey}";
        });
    });
}

// 8. Health Checks (Liveness and Readiness)
builder.Services.AddHealthChecks()
    // Liveness probe - is the container up?
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "liveness" })

    // Readiness probe - can we talk to the database?
    .AddAzureCosmosDB(
    // Resolves your existing factory from DI so it reuses the same underlying connection!
    clientFactory: sp => sp.GetRequiredService<ICosmosClientFactory>().GetClient(),
    name: "cosmosdb-check",
    failureStatus: HealthStatus.Unhealthy,
    tags: ["readiness"]
);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler(); // Uses our IExceptionHandler registration

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Health Check Endpoints
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("liveness")
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("readiness")
});

app.Run();
