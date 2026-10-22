using FluentAssertions;
using Pedidos.Domain.Entities;
using Pedidos.Domain.Enums;
using Pedidos.Domain.Exceptions;

namespace Pedidos.UnitTests.Domain;

public sealed class PedidoTests
{
    private static Pedido CrearPedidoValido(
        string numeroPedido = "PED-001",
        string cliente = "Juan Perez",
        DateTime? fecha = null,
        decimal total = 250.75m,
        EstadoPedido estado = EstadoPedido.Registrado) =>
        new(numeroPedido, cliente, fecha ?? new DateTime(2025, 1, 10), total, estado);

    [Fact]
    public void Constructor_ConValidos_GuardaLosValores()
    {
        var fecha = new DateTime(2025, 1, 10, 8, 30, 0, DateTimeKind.Utc);

        var pedido = CrearPedidoValido(fecha: fecha);

        pedido.Id.Should().Be(0);
        pedido.NumeroPedido.Should().Be("PED-001");
        pedido.Cliente.Should().Be("Juan Perez");
        pedido.Fecha.Should().Be(fecha);
        pedido.Total.Should().Be(250.75m);
        pedido.Estado.Should().Be(EstadoPedido.Registrado);
        pedido.EsEliminado.Should().BeFalse();
        pedido.FechaEliminacion.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_SinNumeroPedido_LanzaExcepcion(string numeroPedido)
    {
        var crear = () => CrearPedidoValido(numeroPedido: numeroPedido);

        crear.Should().Throw<PedidoInvalidoException>()
            .Which.Campo.Should().Be("numeroPedido");
    }

    [Fact]
    public void Constructor_NumeroPedidoDemasiadoLargo_LanzaExcepcion()
    {
        var crear = () => CrearPedidoValido(numeroPedido: new string('A', Pedido.NumeroPedidoMaxLength + 1));

        crear.Should().Throw<PedidoInvalidoException>()
            .Which.Campo.Should().Be("numeroPedido");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_SinCliente_LanzaExcepcion(string cliente)
    {
        var crear = () => CrearPedidoValido(cliente: cliente);

        crear.Should().Throw<PedidoInvalidoException>()
            .Which.Campo.Should().Be("cliente");
    }

    [Fact]
    public void Constructor_ClienteDemasiadoLargo_LanzaExcepcion()
    {
        var crear = () => CrearPedidoValido(cliente: new string('A', Pedido.ClienteMaxLength + 1));

        crear.Should().Throw<PedidoInvalidoException>()
            .Which.Campo.Should().Be("cliente");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void Constructor_TotalNoPositivo_LanzaExcepcion(decimal total)
    {
        var crear = () => CrearPedidoValido(total: total);

        crear.Should().Throw<PedidoInvalidoException>()
            .Which.Campo.Should().Be("total");
    }

    [Fact]
    public void Constructor_FechaPorDefecto_LanzaExcepcion()
    {
        var crear = () => CrearPedidoValido(fecha: DateTime.MinValue);

        crear.Should().Throw<PedidoInvalidoException>()
            .Which.Campo.Should().Be("fecha");
    }

    [Fact]
    public void Constructor_EstadoNoDefinido_LanzaExcepcion()
    {
        var crear = () => CrearPedidoValido(estado: (EstadoPedido)99);

        crear.Should().Throw<PedidoInvalidoException>()
            .Which.Campo.Should().Be("estado");
    }

    [Fact]
    public void Constructor_RecortaLosEspacios()
    {
        var pedido = CrearPedidoValido(numeroPedido: "  PED-002  ", cliente: "  Ana Lopez  ");

        pedido.NumeroPedido.Should().Be("PED-002");
        pedido.Cliente.Should().Be("Ana Lopez");
    }

    [Fact]
    public void Actualizar_AplicaLosNuevosValores()
    {
        var pedido = CrearPedidoValido();

        pedido.Actualizar("PED-999", "Empresa Norte", new DateTime(2025, 3, 1), 99.90m, EstadoPedido.EnProceso);

        pedido.NumeroPedido.Should().Be("PED-999");
        pedido.Cliente.Should().Be("Empresa Norte");
        pedido.Fecha.Should().Be(new DateTime(2025, 3, 1));
        pedido.Total.Should().Be(99.90m);
        pedido.Estado.Should().Be(EstadoPedido.EnProceso);
    }

    [Fact]
    public void Actualizar_TotalNoPositivo_LanzaExcepcion()
    {
        var pedido = CrearPedidoValido();

        var actualizar = () => pedido.Actualizar("PED-001", "Juan Perez", DateTime.Now, 0m, EstadoPedido.Registrado);

        actualizar.Should().Throw<PedidoInvalidoException>()
            .Which.Campo.Should().Be("total");
    }

    [Fact]
    public void MarcarComoEliminado_MarcaElPedidoYEsIdempotente()
    {
        var pedido = CrearPedidoValido();
        var fecha = new DateTime(2025, 5, 5, 10, 0, 0, DateTimeKind.Utc);

        pedido.MarcarComoEliminado(fecha);
        pedido.MarcarComoEliminado(fecha.AddDays(1));

        pedido.EsEliminado.Should().BeTrue();
        pedido.FechaEliminacion.Should().Be(fecha);
    }

    [Fact]
    public void Actualizar_PedidoEliminado_LanzaExcepcion()
    {
        var pedido = CrearPedidoValido();
        pedido.MarcarComoEliminado(DateTime.UtcNow);

        var actualizar = () => pedido.Actualizar("PED-001", "Juan Perez", DateTime.Now, 10m, EstadoPedido.Registrado);

        actualizar.Should().Throw<PedidoInvalidoException>();
    }
}
