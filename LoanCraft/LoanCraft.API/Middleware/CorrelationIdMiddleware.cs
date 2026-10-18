using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace LoanCraft.API.Middleware;

public class CorrelationIdMiddleware
{
    public const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Read incoming X-Correlation-ID from request, or generate a fresh one
        if (!context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId)
            || string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N");
        }

        // 2. Set TraceIdentifier and echo correlation ID in the response headers
        context.TraceIdentifier = correlationId!;
        context.Response.Headers[CorrelationIdHeader] = correlationId;

        // 3. Push CorrelationId into Serilog ambient LogContext so all downstream logs include it
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
