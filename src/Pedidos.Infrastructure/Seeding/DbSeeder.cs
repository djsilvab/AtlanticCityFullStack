using Microsoft.EntityFrameworkCore;
using Pedidos.Application.Auth.Interfaces;
using Pedidos.Domain.Entities;
using Pedidos.Domain.Enums;
using Pedidos.Infrastructure.Persistence;

namespace Pedidos.Infrastructure.Seeding;

/// <summary>
/// Datos iniciales de usuarios. El seed es idempotente: se puede ejecutar en cada arranque.
/// </summary>
public static class DbSeeder
{
    public static async Task SemearAsync(
        AppDbContext context,
        IPasswordHasher passwordHasher,
        CancellationToken cancellationToken = default)
    {
        await SemearUsuarioAsync(context, passwordHasher, "user@email.com", "123456", Rol.User, cancellationToken);
        await SemearUsuarioAsync(context, passwordHasher, "admin@email.com", "Admin123!", Rol.Admin, cancellationToken);
    }

    private static async Task SemearUsuarioAsync(
        AppDbContext context,
        IPasswordHasher passwordHasher,
        string email,
        string password,
        Rol rol,
        CancellationToken cancellationToken)
    {
        var emailNormalizado = Usuario.NormalizarEmail(email);

        var existe = await context.Usuarios
            .AnyAsync(u => u.Email == emailNormalizado, cancellationToken);

        if (existe)
        {
            return;
        }

        var usuario = new Usuario(emailNormalizado, passwordHasher.Hash(password), rol);

        await context.Usuarios.AddAsync(usuario, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
