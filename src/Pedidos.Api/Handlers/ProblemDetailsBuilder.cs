namespace Pedidos.Api.Handlers;

/// <summary>
/// Utilidades para construir respuestas ProblemDetails homogéneas.
/// </summary>
public static class ProblemDetailsBuilder
{
    private const string Base = "https://tools.ietf.org/html/rfc9110#section-";

    /// <summary>Devuelve la URI del tipo de error según el código de estado.</summary>
    public static string TipoPorStatus(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => $"{Base}15.5.1",
        StatusCodes.Status401Unauthorized => $"{Base}15.5.2",
        StatusCodes.Status403Forbidden => $"{Base}15.5.4",
        StatusCodes.Status404NotFound => $"{Base}15.5.5",
        StatusCodes.Status405MethodNotAllowed => $"{Base}15.5.6",
        StatusCodes.Status409Conflict => $"{Base}15.5.10",
        StatusCodes.Status429TooManyRequests => $"{Base}15.5.29",
        _ => $"{Base}15.6.1",
    };

    /// <summary>Convierte el nombre de una propiedad de C# a camelCase (contrato de la API).</summary>
    public static string AJsonCamelCase(string nombre) =>
        string.IsNullOrEmpty(nombre) ? nombre : char.ToLowerInvariant(nombre[0]) + nombre[1..];

    /// <summary>Normaliza las claves de los errores por campo a camelCase.</summary>
    public static IDictionary<string, string[]> NormalizarErrores(IDictionary<string, string[]> errores) =>
        errores.ToDictionary(
            error => AJsonCamelCase(error.Key),
            error => error.Value,
            StringComparer.Ordinal);
}
