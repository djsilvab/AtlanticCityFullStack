using System.Security.Cryptography;

namespace Pedidos.Api.Configuration;

/// <summary>
/// Genera una clave de firma JWT aleatoria para entornos de desarrollo.
/// </summary>
public static class GeneradorClaveJwt
{
    public static string Generar()
    {
        var bytes = RandomNumberGenerator.GetBytes(48);

        return Convert.ToBase64String(bytes);
    }
}
