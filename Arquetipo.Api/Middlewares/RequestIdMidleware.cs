using Microsoft.Extensions.Primitives;

namespace Arquetipo.Api.Middlewares
{
    public class RequestIdMiddleware(RequestDelegate next, ILogger<RequestIdMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<RequestIdMiddleware> _logger = logger;
        public const string RequestIdHeaderName = "X-Request-ID";

        public async Task InvokeAsync(HttpContext context)
        {
            string requestId;

            if (context.Request.Headers.TryGetValue(RequestIdHeaderName, out StringValues existingRequestId) &&
                !StringValues.IsNullOrEmpty(existingRequestId))
            {
                requestId = existingRequestId.ToString();
            }
            else
            {
                requestId = Guid.NewGuid().ToString();
            }

            context.Items[RequestIdHeaderName] = requestId;

            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(RequestIdHeaderName))
                {
                    context.Response.Headers.Append(RequestIdHeaderName, requestId);
                }
                return Task.CompletedTask;
            });

            using (_logger.BeginScope(new Dictionary<string, object>
            {
                [RequestIdHeaderName] = requestId
            }))
            {
                _logger.LogInformation("Procesando solicitud con X-Request-ID: {RequestId}", requestId);
                await _next(context);
            }
        }
    }

    public static class RequestIdMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestId(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestIdMiddleware>();
        }
    }
}
