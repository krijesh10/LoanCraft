using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using LoanCraft.Application.Common.Exceptions;

namespace LoanCraft.API.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = httpContext.TraceIdentifier;

        // 1. Map Exception Type to HTTP Status Code, Title, and Detail
        var (statusCode, title, detail, errors) = exception switch
        {
            ValidationException valEx => (
                StatusCodes.Status400BadRequest,
                "Validation Error",
                valEx.Message,
                valEx.Errors
            ),
            NotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                "Resource Not Found",
                notFoundEx.Message,
                null
            ),
            UnauthorizedAccessException unauthEx => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                unauthEx.Message,
                null
            ),
            ForbiddenException forbiddenEx => (
                StatusCodes.Status403Forbidden,
                "Forbidden",
                forbiddenEx.Message,
                null
            ),
            ConflictException conflictEx => (
                StatusCodes.Status409Conflict,
                "Conflict",
                conflictEx.Message,
                null
            ),
            InvalidOperationException invalidOpEx => (
                StatusCodes.Status409Conflict,
                "Invalid Operation",
                invalidOpEx.Message,
                null
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                _env.IsDevelopment() ? exception.Message : "An unexpected error occurred. Please contact support quoting your Trace ID.",
                null
            )
        };

        // 2. Structured Logging with Serilog
        if (statusCode >= 500)
        {
            _logger.LogError(
                exception,
                "Unhandled Server Error [{StatusCode}] at {Method} {Path}. TraceId: {TraceId}",
                statusCode, httpContext.Request.Method, httpContext.Request.Path, traceId);
        }
        else
        {
            _logger.LogWarning(
                "Handled Application Exception [{StatusCode}] {Title}: {Detail} at {Path}. TraceId: {TraceId}",
                statusCode, title, detail, httpContext.Request.Path, traceId);
        }

        // 3. Assemble RFC 7807 ProblemDetails Response
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        problemDetails.Extensions["traceId"] = traceId;

        // In Development, expose stack trace for easy debugging
        if (_env.IsDevelopment() && exception.StackTrace != null)
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        // If field-level validation errors exist, include them
        if (errors != null && errors.Any())
        {
            problemDetails.Extensions["errors"] = errors;
        }

        // 4. Write JSON response
        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true; // Mark exception as fully handled
    }
}
