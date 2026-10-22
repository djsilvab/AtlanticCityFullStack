using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Pedidos.Application.Auth.Interfaces;
using Pedidos.Application.Auth.Models;
using Pedidos.Domain.Entities;

namespace Pedidos.Infrastructure.Security;

/// <summary>
/// Emisión de tokens JWT con algoritmo HS256.
/// Claims: <c>sub</c> (id), <c>email</c>, <c>role</c>, <c>jti</c> y <c>exp</c>.
/// </summary>
public sealed class JwtTokenService : ITokenService
{
    public const string ClaimRol = "role";
    public const string ClaimEmail = "email";

    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public TokenGenerado CrearToken(Usuario usuario)
    {
        var ahora = DateTime.UtcNow;
        var expira = ahora.AddMinutes(_options.ExpiracionMinutos);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString(CultureInfo.InvariantCulture)),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(ClaimEmail, usuario.Email),
            new(ClaimRol, usuario.Rol.ToString()),
        };

        var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var credenciales = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: ahora,
            expires: expira,
            signingCredentials: credenciales);

        return new TokenGenerado(new JwtSecurityTokenHandler().WriteToken(token), expira);
    }
}
