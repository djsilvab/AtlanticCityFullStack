using Pedidos.Domain.Enums;

namespace Pedidos.Application.Pedidos.DTOs;

/// <summary>
/// Criterios de búsqueda del listado de pedidos.
/// </summary>
public sealed record PedidoConsulta(
    int Pagina = 1,
    int PageSize = 20,
    EstadoPedido? Estado = null,
    string? Cliente = null);
