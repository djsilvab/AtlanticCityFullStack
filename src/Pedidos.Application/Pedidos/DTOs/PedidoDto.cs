using Pedidos.Domain.Enums;

namespace Pedidos.Application.Pedidos.DTOs;

/// <summary>
/// DTO de salida de un pedido.
/// </summary>
public sealed record PedidoDto(
    int Id,
    string NumeroPedido,
    string Cliente,
    DateTime Fecha,
    decimal Total,
    EstadoPedido Estado);
