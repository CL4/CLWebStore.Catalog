using CLWebStore.Catalog.Application.Exceptions;
using CLWebStore.Catalog.Domain.Base;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CLWebStore.Catalog.API.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        var problemDetails = new ProblemDetails
        {
            Instance = httpContext.Request.Path,
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred."
        };

        switch (exception)
        {
            case NotFoundException notFoundException:
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Title = "Resource Not Found";
                problemDetails.Detail = notFoundException.Message;
                break;

            case DomainException domainException:
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Domain Rule Violation";
                problemDetails.Detail = domainException.Message;
                break;

            case ValidationException validationException:
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Validation Failed";
                problemDetails.Extensions["errors"] = validationException.Errors
                    .Select(e => new { e.PropertyName, e.ErrorMessage });
                break;

            case ConcurrencyException concurrencyException:
                // Typically occurs if Cosmos ETag mismatch happens
                problemDetails.Status = StatusCodes.Status409Conflict;
                problemDetails.Title = "Concurrency Conflict";
                problemDetails.Detail = concurrencyException.Message;
                break;
        }

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true; // Indicates the exception has been handled
    }
}
