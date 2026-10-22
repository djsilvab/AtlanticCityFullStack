using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Pedidos.Api.Handlers;
using Pedidos.Infrastructure.Security;

namespace Pedidos.Api.Extensions;

/// <summary>
/// Configuración de la autenticación JWT Bearer.
/// </summary>
public static class AutenticacionExtensions
{
    public static IServiceCollection AddAutenticacionJwt(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, jwt) =>
            {
                // Los claims se leen tal cual (sub, email, role, jti) sin reasignar tipos.
                bearer.MapInboundClaims = false;

                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Value.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Value.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Value.Key)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(jwt.Value.ClockSkewSegundos),
                    NameClaimType = JwtTokenService.ClaimEmail,
                    RoleClaimType = JwtTokenService.ClaimRol,
                };

                bearer.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();

                        await EscribirProblemDetailsAsync(
                            context.HttpContext,
                            StatusCodes.Status401Unauthorized,
                            "No autenticado.",
                            "Las credenciales proporcionadas no son válidas o el token ha expirado.");
                    },
                    OnForbidden = async context =>
                        await EscribirProblemDetailsAsync(
                            context.HttpContext,
                            StatusCodes.Status403Forbidden,
                            "Acceso denegado.",
                            "No tienes permisos para realizar esta operación."),
                };
            });

        services.AddAuthorization();

        return services;
    }

    private static async Task EscribirProblemDetailsAsync(
        HttpContext httpContext,
        int statusCode,
        string titulo,
        string detalle)
    {
        if (httpContext.Response.HasStarted)
        {
            return;
        }

        httpContext.Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Type = ProblemDetailsBuilder.TipoPorStatus(statusCode),
            Status = statusCode,
            Title = titulo,
            Detail = detalle,
            Instance = httpContext.Request.Path,
        };

        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        await httpContext.Response.WriteAsJsonAsync(
            problem,
            options: null,
            contentType: "application/problem+json",
            cancellationToken: httpContext.RequestAborted);
    }
}
