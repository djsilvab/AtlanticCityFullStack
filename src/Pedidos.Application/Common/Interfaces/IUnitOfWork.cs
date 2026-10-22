namespace Pedidos.Application.Common.Interfaces;

/// <summary>
/// Unidad de trabajo: agrupa los cambios de la base de datos en una única transacción.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
