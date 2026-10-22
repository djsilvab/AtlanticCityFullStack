namespace Pedidos.Infrastructure.Security;

/// <summary>
/// Opciones de emisión y validación de tokens JWT. Se configuran desde la sección
/// <c>Jwt</c> de la configuración y se validan al arrancar.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>Clave de firma HS256. Mínimo 32 caracteres.</summary>
    public string Key { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    /// <summary>Vida del token en minutos.</summary>
    public int ExpiracionMinutos { get; set; } = 60;

    /// <summary>Tolerancia de reloj al validar la expiración (segundos).</summary>
    public int ClockSkewSegundos { get; set; } = 30;
}
