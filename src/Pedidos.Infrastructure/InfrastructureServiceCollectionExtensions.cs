using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pedidos.Application.Auth.Interfaces;
using Pedidos.Application.Common.Interfaces;
using Pedidos.Application.Pedidos.Interfaces;
using Pedidos.Infrastructure.Persistence;
using Pedidos.Infrastructure.Repositories;
using Pedidos.Infrastructure.Security;

namespace Pedidos.Infrastructure;

/// <summary>
/// Registro de las implementaciones técnicas de los puertos de Application.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    public const string ConnectionStringPorDefecto =
        "Server=localhost,1433;Database=Pedidos;User Id=sa;Password=;TrustServerCertificate=True;Encrypt=False";

    public static IServiceCollection AddPedidosInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Pedidos");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "No se ha configurado la cadena de conexión 'ConnectionStrings:Pedidos'. " +
                "Defínela en appsettings.json, en el archivo .env (ConnectionStrings__Pedidos) " +
                "o en una variable de entorno.");
        }

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name)));

        services.AddScoped<IPedidoRepository, PedidoRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IPasswordHasher, PasswordHasherService>();

        return services;
    }
}
