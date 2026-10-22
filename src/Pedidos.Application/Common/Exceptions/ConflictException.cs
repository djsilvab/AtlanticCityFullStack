namespace Pedidos.Application.Common.Exceptions;

/// <summary>
/// Conflicto de estado o duplicado. Se traduce a HTTP 409.
/// </summary>
public sealed class ConflictException : Exception
{
    public ConflictException(string mensaje)
        : base(mensaje)
    {
    }
}
