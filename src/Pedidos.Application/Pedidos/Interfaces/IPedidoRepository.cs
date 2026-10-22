using Pedidos.Application.Pedidos.DTOs;
using Pedidos.Domain.Entities;

namespace Pedidos.Application.Pedidos.Interfaces;

/// <summary>
/// Puerto de persistencia de pedidos. Los repositorios no contienen lógica de negocio.
/// </summary>
public interface IPedidoRepository
{
    /// <summary>Listado completo de pedidos no eliminados, del más reciente al más antiguo.</summary>
    Task<IReadOnlyList<Pedido>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Listado paginado y filtrable de pedidos no eliminados.</summary>
    Task<PaginaPedidos> GetPageAsync(PedidoConsulta consulta, CancellationToken cancellationToken = default);

    /// <summary>Lectura de un pedido (sin tracking).</summary>
    Task<Pedido?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Lectura de un pedido para modificarlo (con tracking).</summary>
    Task<Pedido?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Busca por número de pedido, incluidos los eliminados lógicamente (la unicidad lo exige).</summary>
    Task<Pedido?> GetByNumeroPedidoAsync(string numeroPedido, CancellationToken cancellationToken = default);

    /// <summary>Indica si el número de pedido ya está en uso por otro pedido.</summary>
    Task<bool> ExistsNumeroPedidoAsync(string numeroPedido, int? idExcluido = null, CancellationToken cancellationToken = default);

    /// <summary>Añade un pedido al contexto de persistencia.</summary>
    Task AddAsync(Pedido pedido, CancellationToken cancellationToken = default);
}
