namespace Pedidos.Api.Middleware;

/// <summary>
/// Añade cabeceras de seguridad básicas a todas las respuestas.
/// </summary>
public sealed class CabecerasSeguridadMiddleware
{
    private const string ContentTypeOptions = "nosniff";
    private const string FrameOptions = "DENY";
    private const string ReferrerPolicy = "no-referrer";

    private readonly RequestDelegate _next;

    public CabecerasSeguridadMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var cabeceras = context.Response.Headers;

            cabeceras["X-Content-Type-Options"] = ContentTypeOptions;
            cabeceras["X-Frame-Options"] = FrameOptions;
            cabeceras["Referrer-Policy"] = ReferrerPolicy;

            return Task.CompletedTask;
        });

        return _next(context);
    }
}
