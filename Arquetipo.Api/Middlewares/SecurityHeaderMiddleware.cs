namespace Arquetipo.Api.Middlewares
{
    public class SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment env)
    {
        private readonly RequestDelegate _next = next;
        private readonly IWebHostEnvironment _env = env;

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Response.Headers.ContainsKey("X-Content-Type-Options"))
            {
                context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            }

            if (!context.Response.Headers.ContainsKey("Strict-Transport-Security") && context.Request.IsHttps)
            {
                context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
            }

            if (!context.Response.Headers.ContainsKey("Referrer-Policy"))
            {
                context.Response.Headers.Append("Referrer-Policy", "same-origin");
            }

            if (!context.Response.Headers.ContainsKey("Content-Security-Policy"))
            {
                if (_env.IsDevelopment())
                {
                    context.Response.Headers.Append("Content-Security-Policy",
                        "default-src 'self'; " +
                        "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net; " + // Permite scripts en línea, eval (cuidado), y del CDN de Scalar
                        "style-src 'self' 'unsafe-inline'; " + // Permite estilos en línea
                        "connect-src 'self' http://localhost:* ws://localhost:* wss://localhost:*; " + // Permite conexiones a localhost en cualquier puerto para BrowserLink/HotReload
                        "worker-src 'self' blob:; " + // Necesario si Scalar usa web workers desde blobs
                        "img-src 'self' data: https://cdn.jsdelivr.net; " + // Permite imágenes del mismo origen, data URIs y del CDN de Scalar
                        "font-src 'self' https://fonts.gstatic.com https://cdn.jsdelivr.net; " + // Permite fuentes del mismo origen y de Google Fonts/Scalar CDN
                        "frame-ancestors 'none';");
                }
                else
                {
                    // Política más restrictiva para PRODUCCIÓN (ajusta según las necesidades de Scalar si es necesario)
                    context.Response.Headers.Append("Content-Security-Policy",
                        "default-src 'self'; " +
                        "script-src 'self' https://cdn.jsdelivr.net; " +
                        "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
                        "connect-src 'self'; " +
                        "worker-src 'self' blob:; " +
                        "img-src 'self' data: https://cdn.jsdelivr.net; " +
                        "font-src 'self' https://fonts.gstatic.com https://cdn.jsdelivr.net; " +
                        "frame-ancestors 'none';");
                }
            }

            await _next(context);
        }
    }
}