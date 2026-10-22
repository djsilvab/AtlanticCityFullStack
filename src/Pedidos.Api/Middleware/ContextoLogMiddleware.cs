using Serilog.Context;

namespace Pedidos.Api.Middleware;

/// <summary>
/// Enriquece el contexto de log con el identificador de traza y de petición,
/// de modo que todas las líneas de log de una request se puedan correlacionar.
/// </summary>
public sealed class ContextoLogMiddleware
{
    private readonly RequestDelegate _next;

    public ContextoLogMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = System.Diagnostics.Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

        using var propiedadTraceId = LogContext.PushProperty("TraceId", traceId);
        using var propiedadRequestId = LogContext.PushProperty("RequestId", context.TraceIdentifier);

        await _next(context);
    }
}
