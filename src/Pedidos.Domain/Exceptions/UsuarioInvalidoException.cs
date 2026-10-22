namespace Pedidos.Domain.Exceptions;

/// <summary>
/// Se lanza cuando un usuario no cumple las invariantes del dominio.
/// </summary>
public sealed class UsuarioInvalidoException : DomainException
{
    public UsuarioInvalidoException(string campo, string motivo)
        : base($"El valor de '{campo}' no es válido: {motivo}")
    {
        Campo = campo;
    }

    /// <summary>Campo del usuario que ha incumplido la invariante.</summary>
    public string Campo { get; }
}
