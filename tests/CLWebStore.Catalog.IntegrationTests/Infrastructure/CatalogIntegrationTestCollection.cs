namespace CLWebStore.Catalog.IntegrationTests.Infrastructure;

[CollectionDefinition("Catalog Integration", DisableParallelization = true)]
public sealed class CatalogIntegrationTestCollection : ICollectionFixture<CatalogWebApplicationFactory>
{
}
