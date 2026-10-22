using Microsoft.EntityFrameworkCore;
using Pedidos.Application.Common.Mapping;
using Pedidos.Application.Pedidos.DTOs;
using Pedidos.Application.Pedidos.Interfaces;
using Pedidos.Domain.Entities;
using Pedidos.Infrastructure.Persistence;

namespace Pedidos.Infrastructure.Repositories;

/// <summary>
/// Repositorio de pedidos. Solo persiste: ninguna regla de negocio vive aquí.
/// </summary>
public sealed class PedidoRepository : IPedidoRepository
{
    private readonly AppDbContext _context;

    public PedidoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Pedido>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Pedidos
            .AsNoTracking()
            .OrderByDescending(p => p.Fecha)
            .ThenByDescending(p => p.Id)
            .ToListAsync(cancellationToken);

    public async Task<PaginaPedidos> GetPageAsync(PedidoConsulta consulta, CancellationToken cancellationToken = default)
    {
        var pagina = Math.Max(1, consulta.Pagina);
        var pageSize = Math.Clamp(consulta.PageSize, 1, 100);

        var query = _context.Pedidos.AsNoTracking();

        if (consulta.Estado is not null)
        {
            query = query.Where(p => p.Estado == consulta.Estado);
        }

        if (!string.IsNullOrWhiteSpace(consulta.Cliente))
        {
            var cliente = consulta.Cliente.Trim();
            query = query.Where(p => p.Cliente.Contains(cliente));
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.Fecha)
            .ThenByDescending(p => p.Id)
            .Skip((pagina - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var totalPaginas = pageSize == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);

        return new PaginaPedidos(items.ToDto(), pagina, pageSize, total, totalPaginas);
    }

    public Task<Pedido?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Pedidos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Pedido?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Pedidos
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Pedido?> GetByNumeroPedidoAsync(string numeroPedido, CancellationToken cancellationToken = default) =>
        _context.Pedidos
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.NumeroPedido == numeroPedido, cancellationToken);

    public Task<bool> ExistsNumeroPedidoAsync(string numeroPedido, int? idExcluido = null, CancellationToken cancellationToken = default) =>
        _context.Pedidos
            .IgnoreQueryFilters()
            .AnyAsync(p => p.NumeroPedido == numeroPedido && (idExcluido == null || p.Id != idExcluido), cancellationToken);

    public async Task AddAsync(Pedido pedido, CancellationToken cancellationToken = default) =>
        await _context.Pedidos.AddAsync(pedido, cancellationToken);
}
