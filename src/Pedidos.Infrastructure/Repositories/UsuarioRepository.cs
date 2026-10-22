using Microsoft.EntityFrameworkCore;
using Pedidos.Application.Auth.Interfaces;
using Pedidos.Domain.Entities;
using Pedidos.Infrastructure.Persistence;

namespace Pedidos.Infrastructure.Repositories;

/// <summary>
/// Repositorio de usuarios. Solo persiste: ninguna regla de negocio vive aquí.
/// </summary>
public sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == Usuario.NormalizarEmail(email), cancellationToken);

    public Task<Usuario?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _context.Usuarios
            .AnyAsync(u => u.Email == Usuario.NormalizarEmail(email), cancellationToken);

    public async Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default) =>
        await _context.Usuarios.AddAsync(usuario, cancellationToken);
}
