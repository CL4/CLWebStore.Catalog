using System.Diagnostics;

namespace CLWebStore.Catalog.Application.Observability;

public static class CatalogApplicationDiagnostics
{
    public const string ActivitySourceName =
        "CLWebStore.Catalog.Application";

    public static readonly ActivitySource ActivitySource =
        new(ActivitySourceName);
}
