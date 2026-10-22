namespace Pedidos.Application.Common.Exceptions;

/// <summary>
/// Credenciales o token no válidos. Se traduce a HTTP 401 con un mensaje genérico.
/// </summary>
public sealed class UnauthorizedException : Exception
{
    public UnauthorizedException(string mensaje)
        : base(mensaje)
    {
    }
}
