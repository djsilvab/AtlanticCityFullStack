using Pedidos.Application.Common.Interfaces;

namespace Pedidos.Infrastructure.Persistence;

/// <summary>
/// Implementación de la unidad de trabajo sobre <see cref="AppDbContext"/>.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
