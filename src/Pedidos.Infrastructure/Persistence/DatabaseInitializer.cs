using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Pedidos.Infrastructure.Persistence;

/// <summary>
/// Aplica las migraciones al arrancar la API, esperando a que SQL Server esté disponible.
/// </summary>
public static class DatabaseInitializer
{
    private const int MaxIntentos = 10;

    public static async Task MigrarAsync(
        string connectionString,
        IServiceProvider services,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        // La migración usa un contexto propio sin estrategia de reintentos de EF:
        // las migraciones ya son transaccionales y no deben depender de ella.
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using var context = new AppDbContext(options);

        var pipeline = new ResiliencePipelineBuilder()
            // El timeout se aplica a cada intento (estrategia interna) y el reintento
            // envuelve a todos ellos (estrategia externa).
            .AddTimeout(TimeSpan.FromSeconds(60))
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = MaxIntentos,
                Delay = TimeSpan.FromSeconds(2),
                MaxDelay = TimeSpan.FromSeconds(15),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder().Handle<Exception>(EsFalloTransitorio),
                OnRetry = args =>
                {
                    logger.LogWarning(
                        "La base de datos no está disponible (intento {Intento} de {MaxIntentos}): {Error}",
                        args.AttemptNumber + 1,
                        MaxIntentos,
                        args.Outcome.Exception?.Message);

                    return default;
                },
            })
            .Build();

        logger.LogInformation("Aplicando migraciones de la base de datos...");

        await pipeline.ExecuteAsync(
            async token =>
            {
                await context.Database.MigrateAsync(token);
                return true;
            },
            cancellationToken);

        logger.LogInformation("Migraciones aplicadas correctamente.");
    }

    /// <summary>
    /// Detecta los fallos transitorios de SQL Server. EF Core envuelve el error real
    /// (SqlException) en un InvalidOperationException, así que se recorre la cadena.
    /// </summary>
    private static bool EsFalloTransitorio(Exception exception) => exception switch
    {
        SqlException => true,
        TimeoutException => true,
        InvalidOperationException { InnerException: not null } => EsFalloTransitorio(exception.InnerException),
        _ => false,
    };
}
