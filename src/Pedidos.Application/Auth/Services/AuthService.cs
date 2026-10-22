using Pedidos.Application.Auth.DTOs;
using Pedidos.Application.Auth.Interfaces;
using Pedidos.Application.Common.Exceptions;
using Pedidos.Domain.Entities;

namespace Pedidos.Application.Auth.Services;

/// <summary>
/// Caso de uso de autenticación. Valida credenciales y emite el token JWT.
/// </summary>
public sealed class AuthService
{
    /// <summary>Mensaje genérico: no revela si el email existe o la contraseña es incorrecta.</summary>
    public const string MensajeCredencialesInvalidas = "Las credenciales introducidas no son válidas.";

    private readonly IUsuarioRepository _usuarios;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(
        IUsuarioRepository usuarios,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _usuarios = usuarios;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarios.GetByEmailAsync(request.Email, cancellationToken);

        if (usuario is null || !usuario.Activo || !_passwordHasher.Verify(usuario.PasswordHash, request.Password))
        {
            throw new UnauthorizedException(MensajeCredencialesInvalidas);
        }

        var token = _tokenService.CrearToken(usuario);

        var expiraEnSegundos = (int)Math.Max(0, Math.Round((token.ExpiresAt - DateTime.UtcNow).TotalSeconds));

        return new LoginResponse(token.Token, expiraEnSegundos);
    }
}
