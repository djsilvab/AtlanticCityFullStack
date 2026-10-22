using Pedidos.Application.Pedidos.DTOs;
using Pedidos.Domain.Entities;

namespace Pedidos.Application.Common.Mapping;

/// <summary>
/// Mapeo manual entre entidades de dominio y DTOs. Las entidades nunca se exponen en la API.
/// </summary>
public static class PedidoMappings
{
    public static PedidoDto ToDto(this Pedido pedido) =>
        new(
            pedido.Id,
            pedido.NumeroPedido,
            pedido.Cliente,
            pedido.Fecha,
            pedido.Total,
            pedido.Estado);

    public static IReadOnlyList<PedidoDto> ToDto(this IEnumerable<Pedido> pedidos) =>
        pedidos.Select(ToDto).ToList();
}
