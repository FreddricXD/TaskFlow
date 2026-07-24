using System.Net;
using System.Text.Json;
using TaskFlow.Api.Dtos;
using TaskFlow.Api.Services;

namespace TaskFlow.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, logger);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception, ILogger logger)
    {
        var (statusCode, error) = exception switch
        {
            ValidationException validation => (HttpStatusCode.BadRequest, new ApiError(validation.Message, validation.Code)),
            NotFoundException notFound => (HttpStatusCode.NotFound, new ApiError(notFound.Message, notFound.Code)),
            ForbiddenException forbidden => (HttpStatusCode.Forbidden, new ApiError(forbidden.Message, forbidden.Code)),
            ConflictException conflict => (HttpStatusCode.Conflict, new ApiError(conflict.Message, conflict.Code)),
            UnauthorizedAccessException unauthorized => (HttpStatusCode.Unauthorized, new ApiError(unauthorized.Message, "unauthorized")),
            _ => (HttpStatusCode.InternalServerError, new ApiError("An unexpected error occurred.", "server_error"))
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception");
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(error));
    }
}
