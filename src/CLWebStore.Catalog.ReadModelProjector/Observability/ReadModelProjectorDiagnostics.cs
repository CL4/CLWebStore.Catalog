using System.Diagnostics;

namespace CLWebStore.Catalog.ReadModelProjector.Observability;

internal static class ReadModelProjectorDiagnostics
{
    public const string ServiceName = "CLWebStore.Catalog.ReadModelProjector";

    public const string ActivitySourceName = ServiceName;

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
}
