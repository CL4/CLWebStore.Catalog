using CLWebStore.Catalog.Application.Observability;
using MediatR;
using System.Diagnostics;

namespace CLWebStore.Catalog.Application.Behaviors;

public class TracingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        using var activity = CatalogApplicationDiagnostics.ActivitySource.StartActivity($"MediatR {requestName}");
        activity?.SetTag("messaging.operation", "process");
        activity?.SetTag("messaging.system", "mediatr");
        activity?.SetTag("request.type", requestName);

        try
        {
            var response = await next();
            activity?.SetStatus(ActivityStatusCode.Ok);
            return response;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            // Pure BCL implementation of recording an exception (matches OpenTelemetry Semantic Conventions)
            if (activity != null)
            {
                var exceptionTags = new ActivityTagsCollection
                {
                    { "exception.type", ex.GetType().FullName },
                    { "exception.message", ex.Message },
                    { "exception.stacktrace", ex.StackTrace }
                };

                activity.AddEvent(new ActivityEvent("exception", tags: exceptionTags));
            }

            throw;
        }
    }
}
