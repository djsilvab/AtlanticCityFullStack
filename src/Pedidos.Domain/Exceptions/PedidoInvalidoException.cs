namespace Pedidos.Domain.Exceptions;

/// <summary>
/// Se lanza cuando un pedido no cumple las invariantes del dominio.
/// </summary>
public sealed class PedidoInvalidoException : DomainException
{
    public PedidoInvalidoException(string campo, string motivo)
        : base($"El valor de '{campo}' no es válido: {motivo}")
    {
        Campo = campo;
    }

    /// <summary>Campo del pedido que ha incumplido la invariante.</summary>
    public string Campo { get; }
}
