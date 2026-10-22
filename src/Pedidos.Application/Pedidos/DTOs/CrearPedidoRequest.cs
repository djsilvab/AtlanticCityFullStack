using Pedidos.Domain.Enums;

namespace Pedidos.Application.Pedidos.DTOs;

/// <summary>
/// DTO de entrada para crear un pedido.
/// </summary>
public sealed record CrearPedidoRequest(
    string NumeroPedido,
    string Cliente,
    DateTime Fecha,
    decimal Total,
    EstadoPedido Estado);
