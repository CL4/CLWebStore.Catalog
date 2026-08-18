using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace CLWebStore.Catalog.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger = logger;

    // Threshold for what we consider a "slow" request
    private const int SlowRequestThresholdMilliseconds = 500;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Log as Debug to prevent log flooding in production, but include the payload
        // Note: Be mindful of PII/Sensitive data here. You can omit this if strict PII rules apply.
        _logger.LogDebug("Handling {RequestName} with payload: {Request}", requestName, JsonSerializer.Serialize(request));

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();
            stopwatch.Stop();

            // Log completion
            _logger.LogInformation("Handled {RequestName} successfully in {ElapsedMilliseconds}ms", requestName, stopwatch.ElapsedMilliseconds);

            // Flag slow performance explicitly
            if (stopwatch.ElapsedMilliseconds > SlowRequestThresholdMilliseconds)
            {
                _logger.LogWarning(
                    "Long Running Request detected: {RequestName} took {ElapsedMilliseconds}ms. Payload: {Request}",
                    requestName,
                    stopwatch.ElapsedMilliseconds,
                    JsonSerializer.Serialize(request));
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Log the error, the time it took to fail, AND the payload that caused the failure
            _logger.LogError(
                ex,
                "Error handling {RequestName} after {ElapsedMilliseconds}ms. Payload: {Request}",
                requestName,
                stopwatch.ElapsedMilliseconds,
                JsonSerializer.Serialize(request));

            throw;
        }
    }
}
