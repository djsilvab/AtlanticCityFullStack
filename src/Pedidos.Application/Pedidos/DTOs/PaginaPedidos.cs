namespace Pedidos.Application.Pedidos.DTOs;

/// <summary>
/// Resultado paginado de un listado de pedidos.
/// </summary>
public sealed record PaginaPedidos(
    IReadOnlyList<PedidoDto> Items,
    int Pagina,
    int PageSize,
    int Total,
    int TotalPaginas);
