using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pedidos.Application.Auth.Interfaces;
using Pedidos.Infrastructure.Configuration;
using Pedidos.Infrastructure.Persistence;
using Pedidos.Infrastructure.Seeding;

namespace Pedidos.IntegrationTests.Infraestructura;

/// <summary>
/// Host de pruebas: arranca la API real contra la base de datos de test
/// (SQL Server 2022 en Docker) y deja los datos en un estado conocido.
/// </summary>
public sealed class PedidosApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string ConnectionStringPorDefecto =
        "Server=localhost,1433;Database=Pedidos_Tests;User Id=sa;Password=;TrustServerCertificate=True;Encrypt=False";

    private string _connectionString = ConnectionStringPorDefecto;

    public const string EmailUsuario = "user@email.com";
    public const string PasswordUsuario = "123456";
    public const string EmailAdmin = "admin@email.com";
    public const string PasswordAdmin = "Admin123!";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Pedidos"] = _connectionString,
            });
        });
    }

    public async Task InitializeAsync()
    {
        _connectionString = ObtenerConnectionString();

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

    /// <summary>Deja la base de datos en un estado conocido antes de cada prueba.</summary>
    public async Task ReiniciarAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Pedidos.IgnoreQueryFilters().ExecuteDeleteAsync();
    }

    private static string ObtenerConnectionString()
    {
        var desdeEntorno = Environment.GetEnvironmentVariable("ConnectionStrings__PedidosTests");

        if (!string.IsNullOrWhiteSpace(desdeEntorno))
        {
            return desdeEntorno;
        }

        var desdeArchivo = EnvFile.Read();

        if (desdeArchivo.TryGetValue("ConnectionStrings__PedidosTests", out var valor) && !string.IsNullOrWhiteSpace(valor))
        {
            return valor;
        }

        return ConnectionStringPorDefecto;
    }
}
