using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Pedidos.IntegrationTests.Infraestructura;

namespace Pedidos.IntegrationTests.Pedidos;

[Collection(ApiCollection.Nombre)]
public sealed class PedidosEndpointsTests
{
    private readonly PedidosApiFactory _factory;

    public PedidosEndpointsTests(PedidosApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetListado_SinToken_Devuelve401EnFormatoProblemDetails()
    {
        using var cliente = _factory.CrearCliente();

        using var respuesta = await cliente.GetAsync("/api/pedidos");

        respuesta.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        respuesta.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        using var contenido = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        contenido.RootElement.GetProperty("status").GetInt32().Should().Be(401);
        contenido.RootElement.TryGetProperty("traceId", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetListado_ConTokenDevuelve200YLaLista()
    {
        await _factory.ReiniciarAsync();
        using var cliente = await _factory.CrearClienteAutenticadoAsync(
            PedidosApiFactory.EmailUsuario,
            PedidosApiFactory.PasswordUsuario);

        await CrearPedidoAsync(cliente, "PED-100", "Juan Perez", 250.75m);

        using var respuesta = await cliente.GetAsync("/api/pedidos");

        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);

        using var contenido = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        contenido.RootElement.GetArrayLength().Should().Be(1);
        contenido.RootElement[0].GetProperty("numeroPedido").GetString().Should().Be("PED-100");
        contenido.RootElement[0].GetProperty("estado").GetString().Should().Be("Registrado");
    }

    [Fact]
    public async Task GetDetalle_ConIdInexistente_Devuelve404()
    {
        using var cliente = await _factory.CrearClienteAutenticadoAsync(
            PedidosApiFactory.EmailUsuario,
            PedidosApiFactory.PasswordUsuario);

        using var respuesta = await cliente.GetAsync("/api/pedidos/999999");

        respuesta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Post_CrearPedido_Devuelve201ConLocationYElPedidoCreado()
    {
        await _factory.ReiniciarAsync();
        using var cliente = await _factory.CrearClienteAutenticadoAsync(
            PedidosApiFactory.EmailUsuario,
            PedidosApiFactory.PasswordUsuario);

        using var respuesta = await cliente.PostAsJsonAsync(
            "/api/pedidos",
            new { numeroPedido = "PED-200", cliente = "Empresa Norte", fecha = "2025-01-10T00:00:00", total = 99.90m, estado = "Registrado" });

        respuesta.StatusCode.Should().Be(HttpStatusCode.Created);
        respuesta.Headers.Location.Should().NotBeNull();

        using var contenido = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        var id = contenido.RootElement.GetProperty("id").GetInt32();
        contenido.RootElement.GetProperty("cliente").GetString().Should().Be("Empresa Norte");
        contenido.RootElement.GetProperty("total").GetDecimal().Should().Be(99.90m);

        using var detalle = await cliente.GetAsync($"/api/pedidos/{id}");
        detalle.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Post_ConNumeroPedidoDuplicado_Devuelve409()
    {
        await _factory.ReiniciarAsync();
        using var cliente = await _factory.CrearClienteAutenticadoAsync(
            PedidosApiFactory.EmailUsuario,
            PedidosApiFactory.PasswordUsuario);

        await CrearPedidoAsync(cliente, "PED-300", "Juan Perez", 10m);

        using var respuesta = await cliente.PostAsJsonAsync(
            "/api/pedidos",
            new { numeroPedido = "PED-300", cliente = "Otro Cliente", fecha = "2025-01-10T00:00:00", total = 20m, estado = "Registrado" });

        respuesta.StatusCode.Should().Be(HttpStatusCode.Conflict);

        using var contenido = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        contenido.RootElement.GetProperty("status").GetInt32().Should().Be(409);
    }

    [Fact]
    public async Task Post_ConTotalNoPositivo_Devuelve400ConElErrorDelCampoTotal()
    {
        await _factory.ReiniciarAsync();
        using var cliente = await _factory.CrearClienteAutenticadoAsync(
            PedidosApiFactory.EmailUsuario,
            PedidosApiFactory.PasswordUsuario);

        using var respuesta = await cliente.PostAsJsonAsync(
            "/api/pedidos",
            new { numeroPedido = "PED-400", cliente = "Juan Perez", fecha = "2025-01-10T00:00:00", total = 0m, estado = "Registrado" });

        respuesta.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using var contenido = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        var errores = contenido.RootElement.GetProperty("errors");

        errores.TryGetProperty("total", out var total).Should().BeTrue();
        total.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Post_ConCamposVacios_Devuelve400ConLosErroresPorCampo()
    {
        await _factory.ReiniciarAsync();
        using var cliente = await _factory.CrearClienteAutenticadoAsync(
            PedidosApiFactory.EmailUsuario,
            PedidosApiFactory.PasswordUsuario);

        using var respuesta = await cliente.PostAsJsonAsync(
            "/api/pedidos",
            new { numeroPedido = string.Empty, cliente = string.Empty, fecha = "2025-01-10T00:00:00", total = 10m, estado = "Registrado" });

        respuesta.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using var contenido = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        var errores = contenido.RootElement.GetProperty("errors");

        errores.TryGetProperty("numeroPedido", out _).Should().BeTrue();
        errores.TryGetProperty("cliente", out _).Should().BeTrue();
    }

    [Fact]
    public async Task Put_ActualizaElPedidoYDevuelve200()
    {
        await _factory.ReiniciarAsync();
        using var cliente = await _factory.CrearClienteAutenticadoAsync(
            PedidosApiFactory.EmailUsuario,
            PedidosApiFactory.PasswordUsuario);

        var id = await CrearPedidoAsync(cliente, "PED-500", "Juan Perez", 100m);

        using var respuesta = await cliente.PutAsJsonAsync(
            $"/api/pedidos/{id}",
            new { numeroPedido = "PED-500", cliente = "Empresa Norte", fecha = "2025-03-01T00:00:00", total = 150.50m, estado = "EnProceso" });

        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);

        using var contenido = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        contenido.RootElement.GetProperty("cliente").GetString().Should().Be("Empresa Norte");
        contenido.RootElement.GetProperty("total").GetDecimal().Should().Be(150.50m);
        contenido.RootElement.GetProperty("estado").GetString().Should().Be("EnProceso");
    }

    [Fact]
    public async Task Put_ConIdInexistente_Devuelve404()
    {
        using var cliente = await _factory.CrearClienteAutenticadoAsync(
            PedidosApiFactory.EmailUsuario,
            PedidosApiFactory.PasswordUsuario);

        using var respuesta = await cliente.PutAsJsonAsync(
            "/api/pedidos/999999",
            new { numeroPedido = "PED-600", cliente = "Empresa Norte", fecha = "2025-03-01T00:00:00", total = 150.50m, estado = "EnProceso" });

        respuesta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ConRolUser_Devuelve403()
    {
        await _factory.ReiniciarAsync();
        using var cliente = await _factory.CrearClienteAutenticadoAsync(
            PedidosApiFactory.EmailUsuario,
            PedidosApiFactory.PasswordUsuario);

        var id = await CrearPedidoAsync(cliente, "PED-700", "Juan Perez", 100m);

        using var respuesta = await cliente.DeleteAsync($"/api/pedidos/{id}");

        respuesta.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        respuesta.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task Delete_ConRolAdmin_Devuelve204YElPedidoDesapareceDelListado()
    {
        await _factory.ReiniciarAsync();
        using var cliente = await _factory.CrearClienteAutenticadoAsync(
            PedidosApiFactory.EmailAdmin,
            PedidosApiFactory.PasswordAdmin);

        var id = await CrearPedidoAsync(cliente, "PED-800", "Juan Perez", 100m);

        using var respuesta = await cliente.DeleteAsync($"/api/pedidos/{id}");
        respuesta.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var detalle = await cliente.GetAsync($"/api/pedidos/{id}");
        detalle.StatusCode.Should().Be(HttpStatusCode.NotFound);

        using var listado = await cliente.GetAsync("/api/pedidos");
        using var contenido = JsonDocument.Parse(await listado.Content.ReadAsStringAsync());
        contenido.RootElement.GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task Delete_ConIdInexistente_Devuelve404()
    {
        using var cliente = await _factory.CrearClienteAutenticadoAsync(
            PedidosApiFactory.EmailAdmin,
            PedidosApiFactory.PasswordAdmin);

        using var respuesta = await cliente.DeleteAsync("/api/pedidos/999999");

        respuesta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetListadoPaginado_AplicaFiltroPorEstadoYCliente()
    {
        await _factory.ReiniciarAsync();
        using var cliente = await _factory.CrearClienteAutenticadoAsync(
            PedidosApiFactory.EmailUsuario,
            PedidosApiFactory.PasswordUsuario);

        await CrearPedidoAsync(cliente, "PED-900", "Juan Perez", 100m);
        await CrearPedidoAsync(cliente, "PED-901", "Empresa Norte", 200m);

        using var respuesta = await cliente.GetAsync("/api/pedidos/paginado?estado=Registrado&cliente=Empresa");

        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);

        using var contenido = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        contenido.RootElement.GetProperty("total").GetInt32().Should().Be(1);
        contenido.RootElement.GetProperty("items")[0].GetProperty("numeroPedido").GetString().Should().Be("PED-901");
    }

    private static async Task<int> CrearPedidoAsync(HttpClient cliente, string numeroPedido, string nombreCliente, decimal total)
    {
        using var respuesta = await cliente.PostAsJsonAsync(
            "/api/pedidos",
            new { numeroPedido, cliente = nombreCliente, fecha = "2025-01-10T00:00:00", total, estado = "Registrado" });

        respuesta.EnsureSuccessStatusCode();

        using var contenido = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());

        return contenido.RootElement.GetProperty("id").GetInt32();
    }
}
