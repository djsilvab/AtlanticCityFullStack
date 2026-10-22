using Pedidos.Domain.Entities;

namespace Pedidos.Application.Auth.Interfaces;

/// <summary>
/// Puerto de acceso a los datos de usuario.
/// </summary>
public interface IUsuarioRepository
{
    Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<Usuario?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default);
}
