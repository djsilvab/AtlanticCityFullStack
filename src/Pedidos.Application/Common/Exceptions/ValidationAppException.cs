namespace Pedidos.Application.Common.Exceptions;

/// <summary>
/// Error de validación de la capa de aplicación. Se traduce a HTTP 400 con los errores por campo.
/// </summary>
public sealed class ValidationAppException : Exception
{
    public ValidationAppException(IDictionary<string, string[]> errors)
        : base("Uno o más errores de validación se han producido.")
    {
        Errors = errors;
    }

    /// <summary>Errores por campo.</summary>
    public IDictionary<string, string[]> Errors { get; }
}
