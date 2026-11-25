using System.Net;
using System.Text.Json;
using TaskManager.Domain.Exceptions;

namespace TaskManager.Api.Middlewares;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error procesando la solicitud.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            NotFoundException => HttpStatusCode.NotFound,
            BusinessRuleException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };

        var response = new
        {
            status = (int)statusCode,
            error = statusCode.ToString(),
            message = exception.Message
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}