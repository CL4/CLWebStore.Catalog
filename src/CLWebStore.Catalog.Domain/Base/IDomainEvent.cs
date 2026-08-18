namespace CLWebStore.Catalog.Domain.Base;

public interface IDomainEvent
{
    DateTimeOffset OccurredOn
    {
        get;
    }
}
