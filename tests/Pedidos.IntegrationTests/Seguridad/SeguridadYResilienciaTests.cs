using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Pedidos.IntegrationTests.Infraestructura;

namespace Pedidos.IntegrationTests.Seguridad;

[Collection(ApiCollection.Nombre)]
public sealed class SeguridadYResilienciaTests
{
    private readonly PedidosApiFactory _factory;

    public SeguridadYResilienciaTests(PedidosApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Health_EsPublicoYCompruebaLaBaseDeDatos()
    {
        using var cliente = _factory.CrearCliente();

        using var respuesta = await cliente.GetAsync("/health");

        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);

        using var contenido = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        contenido.RootElement.GetProperty("status").GetString().Should().Be("Healthy");

        var comprobaciones = contenido.RootElement.GetProperty("checks");
        comprobaciones.EnumerateArray().Should().Contain(check => check.GetProperty("name").GetString() == "database");
    }

    [Fact]
    public async Task LasRespuestasIncluyenLasCabecerasDeSeguridadBasicas()
    {
        using var cliente = _factory.CrearCliente();

        using var respuesta = await cliente.GetAsync("/health");

        respuesta.Headers.TryGetValues("X-Content-Type-Options", out var contentTypeOptions).Should().BeTrue();
        contentTypeOptions!.Should().ContainSingle().Which.Should().Be("nosniff");

        respuesta.Headers.TryGetValues("X-Frame-Options", out var frameOptions).Should().BeTrue();
        frameOptions!.Should().ContainSingle().Which.Should().Be("DENY");

        respuesta.Headers.TryGetValues("Referrer-Policy", out var referrerPolicy).Should().BeTrue();
        referrerPolicy!.Should().ContainSingle().Which.Should().Be("no-referrer");
    }

    [Fact]
    public async Task ElLoginDevuelve429AlSuperarElLimiteDeIntentos()
    {
        using var factory = new PedidosApiFactoryConLimite(intentosPermitidos: 2);
        await factory.InitializeAsync();

        using var cliente = factory.CreateClient();

        for (var intento = 0; intento < 2; intento++)
        {
            using var permitido = await cliente.PostAsJsonAsync(
                "/auth/login",
                new { email = PedidosApiFactory.EmailAdmin, password = PedidosApiFactory.PasswordAdmin });

            permitido.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        using var rechazado = await cliente.PostAsJsonAsync(
            "/auth/login",
            new { email = PedidosApiFactory.EmailAdmin, password = PedidosApiFactory.PasswordAdmin });

        rechazado.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        rechazado.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        using var contenido = JsonDocument.Parse(await rechazado.Content.ReadAsStringAsync());
        contenido.RootElement.GetProperty("status").GetInt32().Should().Be(429);
    }

    [Fact]
    public async Task UnTokenInvalidoDevuelve401YNoPermiteAcceder()
    {
        using var cliente = _factory.CrearCliente();
        cliente.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "token-invalido");

        using var respuesta = await cliente.GetAsync("/api/pedidos");

        respuesta.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
