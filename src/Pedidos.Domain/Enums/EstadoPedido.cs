namespace Pedidos.Domain.Enums;

/// <summary>
/// Estados válidos de un pedido. Se persisten como texto (HasConversion&lt;string&gt;).
/// </summary>
public enum EstadoPedido
{
    Registrado = 1,
    EnProceso = 2,
    Completado = 3,
    Cancelado = 4
}
