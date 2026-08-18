using System.Diagnostics;

namespace CLWebStore.Catalog.Infrastructure.Observability.Tracing;

public static class ActivitySources
{
    public const string Name = "CLWebStore.Catalog.Infrastructure";
    public static readonly ActivitySource Source = new(Name);
}
