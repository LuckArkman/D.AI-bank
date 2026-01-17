using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace Fintech.Middlewares;

public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDistributedCache _cache;

    public IdempotencyMiddleware(RequestDelegate next, IDistributedCache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (HttpMethods.IsGet(context.Request.Method)) { await _next(context); return; }

        if (!context.Request.Headers.TryGetValue("Idempotency-Key", out var key))
        {
            await _next(context); return;
        }

        var cacheKey = $"idempotency:{key}";
        var cached = await _cache.GetStringAsync(cacheKey);
        
        if (!string.IsNullOrEmpty(cached))
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(cached);
            return;
        }

        // Buffering response
        var originalBody = context.Response.Body;
        using var memStream = new MemoryStream();
        context.Response.Body = memStream;

        await _next(context);

        memStream.Seek(0, SeekOrigin.Begin);
        var responseText = await new StreamReader(memStream).ReadToEndAsync();
        memStream.Seek(0, SeekOrigin.Begin);

        if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
        {
            await _cache.SetStringAsync(cacheKey, responseText, 
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) });
        }

        await memStream.CopyToAsync(originalBody);
    }
}