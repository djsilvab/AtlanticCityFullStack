using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        IServiceProvider services,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = MaxIntentos,
                Delay = TimeSpan.FromSeconds(2),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder()
                    .Handle<SqlException>()
                    .Handle<TimeoutException>()
            })
            .AddTimeout(TimeSpan.FromSeconds(60))
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
}
