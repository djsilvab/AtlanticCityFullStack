using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Pedidos.Api.Configuration;
using Pedidos.Api.Extensions;
using Pedidos.Api.Filters;
using Pedidos.Api.Handlers;
using Pedidos.Application.Auth.DTOs;
using Pedidos.Infrastructure;
using Pedidos.Infrastructure.Configuration;
using Pedidos.Infrastructure.Persistence;
using Pedidos.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;

EnvFile.LoadIntoEnvironment();

var builder = WebApplication.CreateBuilder(args);

// En Development, si no hay clave JWT configurada se genera una efímera en memoria
// para que el proyecto arranque sin secretos versionados (decisión D14).
if (builder.Environment.IsDevelopment() && string.IsNullOrWhiteSpace(builder.Configuration["Jwt:Key"]))
{
    builder.Configuration["Jwt:Key"] = GeneradorClaveJwt.Generar();
}

builder.Services
    .AddControllers(options => options.Filters.Add<ValidacionFluentValidationFilter>())
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
    };
});

builder.Services.AddExceptionHandler<ApiExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Pedidos API",
        Version = "v1",
        Description = "API de autenticación JWT y CRUD de pedidos.",
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduce el token JWT con el formato: Bearer {token}",
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
            },
            Array.Empty<string>()
        },
    });
});

builder.Services.AddPedidosInfrastructure(builder.Configuration);
builder.Services.AddAutenticacionJwt();
builder.Services.AddValidatorsFromAssembly(typeof(LoginRequest).Assembly);
builder.Services.AddScoped<Pedidos.Application.Auth.Services.AuthService>();
builder.Services.AddScoped<Pedidos.Application.Pedidos.Services.PedidoService>();

var app = builder.Build();

if (builder.Environment.IsDevelopment() && string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("Jwt__Key")))
{
    app.Logger.LogWarning(
        "No hay clave JWT configurada: se ha generado una clave efímera para esta sesión. " +
        "Los tokens dejarán de ser válidos al reiniciar la API.");
}

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

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
