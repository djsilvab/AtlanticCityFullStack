namespace Pedidos.Domain.Enums;

/// <summary>
/// Nombres de rol como constantes de tiempo de compilación, para usarlos en
/// los atributos de autorización (<c>[Authorize(Roles = ...)]</c>).
/// </summary>
public static class RolNombre
{
    public const string Admin = nameof(Rol.Admin);

    public const string User = nameof(Rol.User);
}
