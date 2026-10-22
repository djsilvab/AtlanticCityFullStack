using Pedidos.Domain.Enums;
using Pedidos.Domain.Exceptions;

namespace Pedidos.Domain.Entities;

/// <summary>
/// Entidad de usuario. La contraseña solo se almacena como hash.
/// </summary>
public sealed class Usuario
{
    public const int EmailMaxLength = 200;
    public const int PasswordHashMaxLength = 500;

    // Constructor para materialización de EF Core.
    private Usuario()
    {
        Email = string.Empty;
        PasswordHash = string.Empty;
    }

    public int Id { get; private set; }

    public string Email { get; private set; }

    public string PasswordHash { get; private set; }

    public Rol Rol { get; private set; }

    public bool Activo { get; private set; }

    public Usuario(string email, string passwordHash, Rol rol, bool activo = true)
    {
        Email = ValidarEmail(email);
        PasswordHash = ValidarPasswordHash(passwordHash);
        Rol = ValidarRol(rol);
        Activo = activo;
    }

    /// <summary>Normaliza el email para que las búsquedas sean insensibles a mayúsculas.</summary>
    public static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();

    public void Desactivar() => Activo = false;

    public void Activar() => Activo = true;

    private static string ValidarEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new UsuarioInvalidoException("email", "es obligatorio.");
        }

        var valor = NormalizarEmail(email);

        if (valor.Length > EmailMaxLength)
        {
            throw new UsuarioInvalidoException("email", $"no puede superar {EmailMaxLength} caracteres.");
        }

        if (!valor.Contains('@', StringComparison.Ordinal) || valor.Contains(' ', StringComparison.Ordinal))
        {
            throw new UsuarioInvalidoException("email", "no tiene un formato válido.");
        }

        return valor;
    }

    private static string ValidarPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new UsuarioInvalidoException("passwordHash", "es obligatorio.");
        }

        if (passwordHash.Length > PasswordHashMaxLength)
        {
            throw new UsuarioInvalidoException("passwordHash", $"no puede superar {PasswordHashMaxLength} caracteres.");
        }

        return passwordHash;
    }

    private static Rol ValidarRol(Rol rol)
    {
        if (!Enum.IsDefined(rol))
        {
            throw new UsuarioInvalidoException("rol", "no es un rol válido.");
        }

        return rol;
    }
}
