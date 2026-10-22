using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Pedidos.Api.Handlers;

namespace Pedidos.Api.Configuration;

/// <summary>
/// Configuración del rate limiting: limita los intentos de inicio de sesión por IP.
/// </summary>
public static class RateLimitingConfiguracion
{
    public const string PoliticaLogin = "login";

    public static IServiceCollection AddRateLimitingLogin(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(PoliticaLogin, httpContext =>
            {
                // El límite se lee de la configuración en tiempo de ejecución para que
                // cada entorno (incluidos los tests) pueda ajustarlo.
                var configuracion = httpContext.RequestServices.GetRequiredService<IConfiguration>();
                var permitidos = configuracion.GetValue("RateLimiting:LoginPorMinuto", 5);

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocido",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = permitidos,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    });
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                var problem = new ProblemDetails
                {
                    Type = ProblemDetailsBuilder.TipoPorStatus(StatusCodes.Status429TooManyRequests),
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "Demasiadas peticiones.",
                    Detail = "Has superado el número de intentos permitidos. Inténtalo de nuevo en unos instantes.",
                    Instance = context.HttpContext.Request.Path,
                };

                problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

                await context.HttpContext.Response.WriteAsJsonAsync(
                    problem,
                    options: null,
                    contentType: "application/problem+json",
                    cancellationToken: cancellationToken);
            };
        });

        return services;
    }
}
