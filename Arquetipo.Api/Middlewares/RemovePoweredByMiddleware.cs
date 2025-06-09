namespace Arquetipo.Api.Middlewares
{
    public class RemovePoweredByMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;
        private const string PoweredByHeaderName = "X-Powered-By";

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.OnStarting(() =>
            {
                context.Response.Headers.Remove(PoweredByHeaderName);
                return Task.CompletedTask;
            });

            await _next(context);
        }
    }

    public static class RemovePoweredByHeaderMiddlewareExtensions
    {
        public static IApplicationBuilder UseRemovePoweredByHeader(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RemovePoweredByMiddleware>();
        }
    }
}