using CLWebStore.Catalog.Application.Abstractions;
using CLWebStore.Catalog.Application.Abstractions.V1;
using CLWebStore.Catalog.Infrastructure.Persistence.Cosmos;
using CLWebStore.Catalog.Infrastructure.QueryServices.V1;
using CLWebStore.Catalog.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Data;

namespace CLWebStore.Catalog.Infrastructure.DependencyInjection;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CosmosSettings>(configuration.GetSection("CosmosSettings"));
        services.AddSingleton<CosmosClientFactory>();
        services.AddScoped<IProductRepository, ProductRepository>();

        // Register read-model DB connection for PostgreSQL (Npgsql). Assumes a connection string named "ReadDatabase" is present in configuration.
        services.AddTransient<IDbConnection>(sp => new NpgsqlConnection(configuration.GetConnectionString("ReadDatabase")));

        // Register the Dapper-based product query service
        services.AddScoped<IProductQueryService, ProductQueryService>();

        return services;
    }
}
