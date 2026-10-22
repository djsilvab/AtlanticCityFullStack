using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pedidos.Application.Auth.Interfaces;
using Pedidos.Infrastructure.Persistence;
using Pedidos.Infrastructure.Seeding;

namespace Pedidos.IntegrationTests.Infraestructura;

/// <summary>
/// Host de pruebas con un límite de intentos de login bajo, para verificar el rate limiting.
/// </summary>
public sealed class PedidosApiFactoryConLimite : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly int _intentosPermitidos;

    public PedidosApiFactoryConLimite(int intentosPermitidos = 2)
    {
        _intentosPermitidos = intentosPermitidos;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Pedidos"] = ConexionTest.Obtener(),
                ["RateLimiting:LoginPorMinuto"] = _intentosPermitidos.ToString(),
            });
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await context.Database.MigrateAsync();
        await DbSeeder.SemearAsync(context, passwordHasher);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        Dispose();
        await Task.CompletedTask;
    }
}
