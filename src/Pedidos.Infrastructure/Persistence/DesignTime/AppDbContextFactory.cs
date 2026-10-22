using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Pedidos.Infrastructure.Configuration;

namespace Pedidos.Infrastructure.Persistence.DesignTime;

/// <summary>
/// Factoría de diseño: permite a <c>dotnet ef</c> crear el contexto sin levantar la API.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    private const string ConnectionStringPorDefecto =
        "Server=localhost,1433;Database=Pedidos;User Id=sa;Password=;TrustServerCertificate=True;Encrypt=False";

    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(ObtenerConnectionString());

        return new AppDbContext(optionsBuilder.Options);
    }

    private static string ObtenerConnectionString()
    {
        var desdeEntorno = Environment.GetEnvironmentVariable("ConnectionStrings__Pedidos");

        if (!string.IsNullOrWhiteSpace(desdeEntorno))
        {
            return desdeEntorno;
        }

        var desdeArchivo = EnvFile.Read();

        if (desdeArchivo.TryGetValue("ConnectionStrings__Pedidos", out var valor) && !string.IsNullOrWhiteSpace(valor))
        {
            return valor;
        }

        return ConnectionStringPorDefecto;
    }
}
