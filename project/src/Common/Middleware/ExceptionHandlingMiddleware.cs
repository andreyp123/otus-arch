using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using Common.Helpers;
using Common.Model;

namespace Common.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next.Invoke(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.StatusCode = exception switch
        {
            CrashConflictException => (int) HttpStatusCode.Conflict,
            CrashException => (int) HttpStatusCode.BadRequest,
            _ => (int) HttpStatusCode.InternalServerError
        };

        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(
            JsonHelper.Serialize(ErrorResult.FromException(exception)));
    }
}