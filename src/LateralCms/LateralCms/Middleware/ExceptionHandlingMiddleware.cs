using LateralCms.Api.Contracts.Responses;
using LateralCms.Domain.Exceptions;
using Microsoft.AspNetCore.WebUtilities;

namespace LateralCms.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException domainEx)
        {
            _logger.LogError(
                domainEx,
                "Domain exception while processing {Method} {Path}. Trace identifier: {TraceIdentifier}",
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier);

            await WrapResponse(context, 400, domainEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled exception while processing {Method} {Path}. Trace identifier: {TraceIdentifier}",
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier);

            if (context.Response.HasStarted)
            {
                throw;
            }

            await WrapResponse(context, 500, ex);
        }
    }

    private static async Task WrapResponse(HttpContext context, int statusCode, Exception ex)
    {
        context.Response.Clear();
        context.Response.StatusCode = statusCode;

        var response = new ResponseWrapper
        {
            Success = false,
            HttpStatusCode = statusCode,
            HttpStatus = ReasonPhrases.GetReasonPhrase(statusCode),
            Message = ex.Message,
            Data = null
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}
