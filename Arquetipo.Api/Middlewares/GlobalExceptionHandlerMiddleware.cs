using System.Net;
using System.Text.Json;

namespace Arquetipo.Api.Middlewares;

public class GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger, IWebHostEnvironment env)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger = logger;
    private readonly IWebHostEnvironment _env = env;

    public async Task InvokeAsync(HttpContext context)
    {
        string requestId = context.Items[RequestIdMiddleware.RequestIdHeaderName] as string ?? "N/A-From-ExceptionHandler";
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocurrió una excepción no controlada (X-Request-ID: {RequestId}): {Message}", requestId, ex.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            object response;
            if (_env.IsDevelopment())
            {
                response = new
                {
                    context.Response.StatusCode,
                    ex.Message,
                    InnerException = ex.InnerException?.Message,
                    StackTrace = ex.StackTrace?.ToString()
                };
            }
            else
            {
                response = new
                {
                    context.Response.StatusCode,
                    Message = "Ocurrió un error interno en el servidor."
                };
            }
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}