using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Pedidos.Application.Common.Exceptions;
using Pedidos.Application.Common.Interfaces;

namespace Pedidos.Infrastructure.Persistence;

/// <summary>
/// Implementación de la unidad de trabajo sobre <see cref="AppDbContext"/>.
/// Traduce la violación del índice único de SQL Server a un conflicto de negocio (HTTP 409).
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private const int ErrorIndiceUnico = 2601;
    private const int ErrorRestriccionUnica = 2627;

    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (EsViolacionDeValorUnico(exception))
        {
            throw new ConflictException("Ya existe un registro con ese valor único.");
        }
    }

    private static bool EsViolacionDeValorUnico(DbUpdateException exception) =>
        exception.InnerException is SqlException sqlException &&
        sqlException.Number is ErrorIndiceUnico or ErrorRestriccionUnica;
}
