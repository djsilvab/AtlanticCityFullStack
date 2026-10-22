using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Pedidos.Api.Configuration;

/// <summary>
/// Escribe la respuesta del health check en formato JSON.
/// </summary>
public static class HealthCheckRespuesta
{
    public static Task EscribirAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var respuesta = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(entrada => new
            {
                name = entrada.Key,
                status = entrada.Value.Status.ToString(),
                duration = entrada.Value.Duration.TotalMilliseconds,
                description = entrada.Value.Description,
            }),
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(respuesta));
    }
}
