using ReadModelSeeder.Cosmos;
using ReadModelSeeder.PostgreSql;
using ReadModelSeeder.Services;
using ReadModelSeeder.Settings;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Serilog;

namespace ReadModelSeeder;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellationTokenSource.Cancel();
        };

        try
        {
            using IHost host = Host.CreateDefaultBuilder(args)
                .UseSerilog((context, _, loggerConfiguration) =>
                    loggerConfiguration.ReadFrom.Configuration(context.Configuration))
                .ConfigureServices((context, services) =>
                {
                    services.AddOptions<MigrationSettings>()
                        .Bind(context.Configuration.GetSection(MigrationSettings.SectionName))
                        .Validate(settings => !string.IsNullOrWhiteSpace(settings.CosmosDbDatabaseName),
                            "MigrationSettings:CosmosDbDatabaseName is required.")
                        .Validate(settings => !string.IsNullOrWhiteSpace(settings.CosmosDbContainerName),
                            "MigrationSettings:CosmosDbContainerName is required.")
                        .ValidateOnStart();

                    services.AddSingleton(serviceProvider =>
                    {
                        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                        return new CosmosClient(GetRequiredConnectionString(configuration, "CosmosDb"));
                    });

                    services.AddSingleton(serviceProvider =>
                    {
                        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                        var builder = new NpgsqlDataSourceBuilder(
                            GetRequiredConnectionString(configuration, "Postgres"));

                        return builder.Build();
                    });

                    services.AddSingleton<ICosmosProductReader, CosmosProductReader>();
                    services.AddSingleton<IProductReadModelWriter, ProductReadModelWriter>();
                    services.AddSingleton<ProductMigrationService>();
                })
                .Build();

            await host.StartAsync(cancellationTokenSource.Token);

            try
            {
                var migrationService = host.Services.GetRequiredService<ProductMigrationService>();
                await migrationService.RunAsync(cancellationTokenSource.Token);
            }
            finally
            {
                await host.StopAsync(CancellationToken.None);
            }

            return 0;
        }
        catch (OperationCanceledException) when (cancellationTokenSource.IsCancellationRequested)
        {
            Console.Error.WriteLine("Product read model migration was cancelled.");
            return 1;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Product read model migration failed: {exception.Message}");
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static string GetRequiredConnectionString(IConfiguration configuration, string name)
    {
        var connectionString = configuration.GetConnectionString(name);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"Connection string '{name}' is required.");
        }

        return connectionString;
    }
}
