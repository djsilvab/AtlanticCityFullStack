namespace Pedidos.Domain.Exceptions;

/// <summary>
/// Base de las excepciones de dominio: expresan violaciones de invariantes del negocio.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message)
        : base(message)
    {
    }
}
