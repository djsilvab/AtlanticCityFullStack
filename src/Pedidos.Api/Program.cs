using System.Text.Json.Serialization;
using FluentValidation;
using Pedidos.Application.Auth.DTOs;
using Pedidos.Infrastructure;
using Pedidos.Infrastructure.Configuration;
using Pedidos.Infrastructure.Persistence;
using Pedidos.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;

EnvFile.LoadIntoEnvironment();

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddPedidosInfrastructure(builder.Configuration);
builder.Services.AddValidatorsFromAssembly(typeof(LoginRequest).Assembly);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

// En el entorno de test la base de datos la prepara el fixture de integración.
if (!app.Environment.IsEnvironment("Testing"))
{
    await InicializarBaseDeDatosAsync(app);
}

app.Run();

static async Task InicializarBaseDeDatosAsync(WebApplication app)
{
    await DatabaseInitializer.MigrarAsync(app.Services, app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Database"));

    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<Pedidos.Application.Auth.Interfaces.IPasswordHasher>();

        await DbSeeder.SemearAsync(context, passwordHasher);
    }
}

/// <summary>
/// Punto de entrada accesible para <c>WebApplicationFactory</c> en los tests de integración.
/// </summary>
public partial class Program
{
}
