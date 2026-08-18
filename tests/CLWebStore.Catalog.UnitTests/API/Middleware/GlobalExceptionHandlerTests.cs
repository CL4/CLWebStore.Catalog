using CLWebStore.Catalog.API.Middleware;
using CLWebStore.Catalog.Application.Exceptions;
using CLWebStore.Catalog.Domain.Base;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace CLWebStore.Catalog.UnitTests.API.Middleware;

public class GlobalExceptionHandlerTests
{
    private static DefaultHttpContext CreateHttpContext()
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();
        ctx.Request.Path = "/test";
        return ctx;
    }

    private static async Task<JsonDocument> GetResponseJson(HttpContext ctx)
    {
        ctx.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(ctx.Response.Body);
        var text = await reader.ReadToEndAsync();
        return JsonDocument.Parse(text);
    }

    [Fact]
    public async Task TryHandleAsync_GenericException_Returns500()
    {
        var mockLogger = new Mock<ILogger<GlobalExceptionHandler>>();
        var handler = new GlobalExceptionHandler(mockLogger.Object);

        var ctx = CreateHttpContext();
        var ex = new Exception("boom");

        var handled = await handler.TryHandleAsync(ctx, ex, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, ctx.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_NotFoundException_Returns404()
    {
        var mockLogger = new Mock<ILogger<GlobalExceptionHandler>>();
        var handler = new GlobalExceptionHandler(mockLogger.Object);

        var ctx = CreateHttpContext();
        var ex = new NotFoundException("Entity", Guid.NewGuid());

        var handled = await handler.TryHandleAsync(ctx, ex, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status404NotFound, ctx.Response.StatusCode);

        var json = await GetResponseJson(ctx);
        Assert.Equal("Resource Not Found", json.RootElement.GetProperty("title").GetString());
    }

    [Fact]
    public async Task TryHandleAsync_DomainException_Returns400()
    {
        var mockLogger = new Mock<ILogger<GlobalExceptionHandler>>();
        var handler = new GlobalExceptionHandler(mockLogger.Object);

        var ctx = CreateHttpContext();
        var ex = new DomainException("invalid");

        var handled = await handler.TryHandleAsync(ctx, ex, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status400BadRequest, ctx.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_ValidationException_Returns400_And_IncludesErrors()
    {
        var mockLogger = new Mock<ILogger<GlobalExceptionHandler>>();
        var handler = new GlobalExceptionHandler(mockLogger.Object);

        var failures = new[] { new ValidationFailure("Name", "Required") };
        var validationException = new ValidationException(failures);

        var ctx = CreateHttpContext();

        var handled = await handler.TryHandleAsync(ctx, validationException, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status400BadRequest, ctx.Response.StatusCode);

        var json = await GetResponseJson(ctx);
        var raw = json.RootElement.GetRawText();
        Assert.Contains("Required", raw, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TryHandleAsync_ConcurrencyException_Returns409()
    {
        var mockLogger = new Mock<ILogger<GlobalExceptionHandler>>();
        var handler = new GlobalExceptionHandler(mockLogger.Object);

        var ctx = CreateHttpContext();
        var ex = new ConcurrencyException("conflict");

        var handled = await handler.TryHandleAsync(ctx, ex, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status409Conflict, ctx.Response.StatusCode);
    }
}
