using CLWebStore.Catalog.Application.Abstractions;
using CLWebStore.Catalog.Application.Abstractions.V1;
using CLWebStore.Catalog.Application.DTOs.V1; // Required for ProductImageDto
using CLWebStore.Catalog.Infrastructure.Persistence.Cosmos;
using CLWebStore.Catalog.Infrastructure.QueryServices.V1;
using CLWebStore.Catalog.Infrastructure.Repositories;
using Dapper; // Required for SqlMapper
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
        services.AddSingleton<ICosmosClientFactory, CosmosClientFactory>();
        services.AddScoped<IProductRepository, ProductRepository>();

        // Register read-model DB connection for PostgreSQL (Npgsql).
        services.AddTransient<IDbConnection>(sp => new NpgsqlConnection(configuration.GetConnectionString("PostgreSQL")));

        // Register the Dapper-based product query service
        services.AddScoped<IProductQueryService, ProductQueryService>();

        // Register Dapper Type Handlers globally for PostgreSQL JSONB
        SqlMapper.AddTypeHandler(new JsonTypeHandler<List<ProductImageDto>>());

        return services;
    }
}