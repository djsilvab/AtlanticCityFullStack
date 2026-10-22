using Pedidos.Application.Auth.Models;
using Pedidos.Domain.Entities;

namespace Pedidos.Application.Auth.Interfaces;

/// <summary>
/// Puerto de emisión de tokens JWT.
/// </summary>
public interface ITokenService
{
    TokenGenerado CrearToken(Usuario usuario);
}
