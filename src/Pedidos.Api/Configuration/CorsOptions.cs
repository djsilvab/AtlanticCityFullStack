namespace Pedidos.Api.Configuration;

/// <summary>
/// Opciones de CORS: los orígenes permitidos son los del frontend configurados en
/// <c>Cors:AllowedOrigins</c>.
/// </summary>
public sealed class CorsOptions
{
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; set; } = ["http://localhost:5173"];
}
