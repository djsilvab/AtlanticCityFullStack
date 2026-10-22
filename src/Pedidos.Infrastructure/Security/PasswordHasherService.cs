using Microsoft.AspNetCore.Identity;
using Pedidos.Application.Auth.Interfaces;
using Pedidos.Domain.Entities;
using Pedidos.Domain.Enums;

namespace Pedidos.Infrastructure.Security;

/// <summary>
/// Hash y verificación de contraseñas con el algoritmo de ASP.NET Core Identity
/// (PBKDF2 con HMAC-SHA256 y sal aleatoria por usuario).
/// </summary>
public sealed class PasswordHasherService : IPasswordHasher
{
    // El hasher solo usa el usuario para calcular el discriminador de contraseña, que en
    // nuestro caso es siempre null. Se reutiliza una instancia para no crear una entidad por hash.
    private static readonly Usuario Contexto = new("hash@localhost.invalid", "0", Rol.User);

    private readonly PasswordHasher<Usuario> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(Contexto, password);

    public bool Verify(string hash, string password) => _hasher.VerifyHashedPassword(Contexto, hash, password) != PasswordVerificationResult.Failed;
}
