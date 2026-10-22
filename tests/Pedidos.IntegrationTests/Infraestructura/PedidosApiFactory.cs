using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pedidos.Application.Auth.DTOs;
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
    private const string ConnectionStringPorDefecto = ConexionTest.ConnectionStringPorDefecto;

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

                // En las pruebas el límite de intentos se agota de forma intencionada
                // en un caso concreto; el resto usa un margen amplio.
                ["RateLimiting:LoginPorMinuto"] = "1000",
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

    /// <summary>Cliente HTTP contra la API de pruebas.</summary>
    public HttpClient CrearCliente() => CreateClient();

    /// <summary>Inicia sesión y devuelve el token JWT.</summary>
    public async Task<string> ObtenerTokenAsync(string email, string password)
    {
        using var cliente = CrearCliente();

        using var respuesta = await cliente.PostAsJsonAsync("/auth/login", new { email, password });

        respuesta.EnsureSuccessStatusCode();

        var contenido = await respuesta.Content.ReadFromJsonAsync<LoginResponse>();

        return contenido!.Token;
    }

    /// <summary>Cliente HTTP con el token de un usuario concreto.</summary>
    public async Task<HttpClient> CrearClienteAutenticadoAsync(string email, string password)
    {
        var token = await ObtenerTokenAsync(email, password);
        var cliente = CrearCliente();

        cliente.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        return cliente;
    }

    private static string ObtenerConnectionString() => ConexionTest.Obtener();
}
