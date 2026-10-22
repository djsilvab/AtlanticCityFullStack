using Pedidos.Infrastructure.Configuration;

namespace Pedidos.IntegrationTests.Infraestructura;

/// <summary>
/// Resuelve la cadena de conexión de la base de datos de pruebas.
/// Prioridad: variable de entorno → archivo .env → valor por defecto.
/// </summary>
public static class ConexionTest
{
    public const string ConnectionStringPorDefecto =
        "Server=localhost,1433;Database=Pedidos_Tests;User Id=sa;Password=;TrustServerCertificate=True;Encrypt=False";

    public static string Obtener()
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
