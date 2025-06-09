using Arquetipo.Api.Middlewares;

namespace Arquetipo.Api.HttpHandlers;

public class ForwardRequestIdDelegatingHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext != null && httpContext.Items.TryGetValue(RequestIdMiddleware.RequestIdHeaderName, out var requestIdObj) &&
            requestIdObj is string requestId && !string.IsNullOrEmpty(requestId))
        {
            if (!request.Headers.Contains(RequestIdMiddleware.RequestIdHeaderName))
            {
                request.Headers.Add(RequestIdMiddleware.RequestIdHeaderName, requestId);
            }
        }
        return await base.SendAsync(request, cancellationToken);
    }
}