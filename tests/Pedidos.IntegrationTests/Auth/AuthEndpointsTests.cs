using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Pedidos.IntegrationTests.Infraestructura;

namespace Pedidos.IntegrationTests.Auth;

[Collection(ApiCollection.Nombre)]
public sealed class AuthEndpointsTests
{
    private readonly PedidosApiFactory _factory;

    public AuthEndpointsTests(PedidosApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_ConCredencialesValidas_Devuelve200ConTokenYExpiracion()
    {
        using var cliente = _factory.CrearCliente();

        using var respuesta = await cliente.PostAsJsonAsync(
            "/auth/login",
            new { email = PedidosApiFactory.EmailAdmin, password = PedidosApiFactory.PasswordAdmin });

        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);

        using var contenido = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        contenido.RootElement.GetProperty("token").GetString().Should().NotBeNullOrWhiteSpace();
        contenido.RootElement.GetProperty("expiresIn").GetInt32().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Login_ElTokenIncluyeLosClaimsExigidos()
    {
        var token = await _factory.ObtenerTokenAsync(PedidosApiFactory.EmailAdmin, PedidosApiFactory.PasswordAdmin);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.Claims.Should().Contain(claim => claim.Type == JwtRegisteredClaimNames.Sub);
        jwt.Claims.Should().Contain(claim => claim.Type == JwtRegisteredClaimNames.Jti);
        jwt.Claims.Should().Contain(claim => claim.Type == "email" && claim.Value == PedidosApiFactory.EmailAdmin);
        jwt.Claims.Should().Contain(claim => claim.Type == "role" && claim.Value == "Admin");
        jwt.ValidTo.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_ConPasswordIncorrecta_Devuelve401ConMensajeGenerico()
    {
        using var cliente = _factory.CrearCliente();

        using var respuesta = await cliente.PostAsJsonAsync(
            "/auth/login",
            new { email = PedidosApiFactory.EmailAdmin, password = "password-incorrecta" });

        respuesta.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        respuesta.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        using var contenido = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        contenido.RootElement.GetProperty("status").GetInt32().Should().Be(401);
        contenido.RootElement.GetProperty("detail").GetString().Should().NotContain(PedidosApiFactory.EmailAdmin);
    }

    [Fact]
    public async Task Login_ConUsuarioInexistente_Devuelve401ConElMismoMensaje()
    {
        using var cliente = _factory.CrearCliente();

        using var respuesta = await cliente.PostAsJsonAsync(
            "/auth/login",
            new { email = "nadie@email.com", password = "123456" });

        respuesta.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        using var contenido = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        contenido.RootElement.GetProperty("detail").GetString()
            .Should().Be("Las credenciales introducidas no son válidas.");
    }

    [Fact]
    public async Task Login_ConPeticionInvalida_Devuelve400ConErroresPorCampo()
    {
        using var cliente = _factory.CrearCliente();

        using var respuesta = await cliente.PostAsJsonAsync(
            "/auth/login",
            new { email = "no-es-un-email", password = string.Empty });

        respuesta.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using var contenido = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        var errores = contenido.RootElement.GetProperty("errors");

        errores.TryGetProperty("email", out _).Should().BeTrue();
        errores.TryGetProperty("password", out _).Should().BeTrue();
    }

    [Fact]
    public async Task Login_ConTokenValidoPermiteAccederAUnEndpointProtegido()
    {
        using var cliente = await _factory.CrearClienteAutenticadoAsync(
            PedidosApiFactory.EmailUsuario,
            PedidosApiFactory.PasswordUsuario);

        using var respuesta = await cliente.GetAsync("/api/pedidos");

        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
