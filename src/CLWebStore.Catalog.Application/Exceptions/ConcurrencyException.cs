namespace CLWebStore.Catalog.Application.Exceptions;

public sealed class ConcurrencyException(string message, Exception? innerException = null)
    : Exception(message, innerException)
{
}
