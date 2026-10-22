using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pedidos.Domain.Enums;
using Pedidos.Infrastructure.Persistence;
using Pedidos.IntegrationTests.Infraestructura;

namespace Pedidos.IntegrationTests.Persistencia;

[Collection(ApiCollection.Nombre)]
public sealed class MigracionTests
{
    private readonly PedidosApiFactory _factory;

    public MigracionTests(PedidosApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task LaMigracionCreaLasTablasPedidosYUsuarios()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        (await context.Pedidos.CountAsync()).Should().BeGreaterThanOrEqualTo(0);
        (await context.Usuarios.CountAsync()).Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task ElSeedCreaLosUsuariosDePruebaConSuRol()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var usuario = await context.Usuarios.SingleAsync(u => u.Email == PedidosApiFactory.EmailUsuario);
        var admin = await context.Usuarios.SingleAsync(u => u.Email == PedidosApiFactory.EmailAdmin);

        usuario.Rol.Should().Be(Rol.User);
        admin.Rol.Should().Be(Rol.Admin);
        usuario.PasswordHash.Should().NotBeNullOrWhiteSpace();
        usuario.PasswordHash.Should().NotBe(PedidosApiFactory.PasswordUsuario);
    }
}
