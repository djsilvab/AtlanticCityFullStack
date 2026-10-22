using Pedidos.Domain.Enums;

namespace Pedidos.Application.Pedidos.DTOs;

/// <summary>
/// DTO de entrada para actualizar un pedido. El identificador de la ruta manda sobre el del cuerpo.
/// </summary>
public sealed record ActualizarPedidoRequest(
    string NumeroPedido,
    string Cliente,
    DateTime Fecha,
    decimal Total,
    EstadoPedido Estado);
