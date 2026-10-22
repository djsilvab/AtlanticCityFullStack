using FluentAssertions;
using Pedidos.Application.Pedidos.DTOs;
using Pedidos.Application.Pedidos.Validators;
using Pedidos.Domain.Enums;

namespace Pedidos.UnitTests.Application.Validators;

public sealed class PedidoRequestValidatorTests
{
    private readonly CrearPedidoRequestValidator _crearValidator = new();
    private readonly ActualizarPedidoRequestValidator _actualizarValidator = new();

    [Fact]
    public void Crear_PedidoValido_NoDevuelveErrores()
    {
        var resultado = _crearValidator.Validate(CrearPedidoValido());

        resultado.IsValid.Should().BeTrue();
        resultado.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Crear_TotalNoPositivo_InvalidaElCampoTotal()
    {
        var resultado = _crearValidator.Validate(CrearPedidoValido() with { Total = 0m });

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().ContainSingle(e => e.PropertyName == nameof(CrearPedidoRequest.Total));
    }

    [Fact]
    public void Crear_TotalConMasDeDosDecimales_InvalidaElCampoTotal()
    {
        var resultado = _crearValidator.Validate(CrearPedidoValido() with { Total = 10.123m });

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().ContainSingle(e => e.PropertyName == nameof(CrearPedidoRequest.Total));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_SinNumeroPedido_InvalidaElCampoNumeroPedido(string numeroPedido)
    {
        var resultado = _crearValidator.Validate(CrearPedidoValido() with { NumeroPedido = numeroPedido });

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().ContainSingle(e => e.PropertyName == nameof(CrearPedidoRequest.NumeroPedido));
    }

    [Fact]
    public void Crear_NumeroPedidoDemasiadoLargo_InvalidaElCampoNumeroPedido()
    {
        var resultado = _crearValidator.Validate(CrearPedidoValido() with { NumeroPedido = new string('A', 51) });

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().ContainSingle(e => e.PropertyName == nameof(CrearPedidoRequest.NumeroPedido));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_SinCliente_InvalidaElCampoCliente(string cliente)
    {
        var resultado = _crearValidator.Validate(CrearPedidoValido() with { Cliente = cliente });

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().ContainSingle(e => e.PropertyName == nameof(CrearPedidoRequest.Cliente));
    }

    [Fact]
    public void Crear_EstadoNoValido_InvalidaElCampoEstado()
    {
        var resultado = _crearValidator.Validate(CrearPedidoValido() with { Estado = (EstadoPedido)99 });

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().ContainSingle(e => e.PropertyName == nameof(CrearPedidoRequest.Estado));
    }

    [Fact]
    public void Crear_FechaPorDefecto_InvalidaElCampoFecha()
    {
        var resultado = _crearValidator.Validate(CrearPedidoValido() with { Fecha = default });

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().ContainSingle(e => e.PropertyName == nameof(CrearPedidoRequest.Fecha));
    }

    [Fact]
    public void Actualizar_PedidoValido_NoDevuelveErrores()
    {
        var resultado = _actualizarValidator.Validate(ActualizarPedidoValido());

        resultado.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Actualizar_TotalNoPositivo_InvalidaElCampoTotal()
    {
        var resultado = _actualizarValidator.Validate(ActualizarPedidoValido() with { Total = -5m });

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().ContainSingle(e => e.PropertyName == nameof(ActualizarPedidoRequest.Total));
    }

    private static CrearPedidoRequest CrearPedidoValido() =>
        new("PED-001", "Juan Perez", new DateTime(2025, 1, 10), 250.75m, EstadoPedido.Registrado);

    private static ActualizarPedidoRequest ActualizarPedidoValido() =>
        new("PED-001", "Juan Perez", new DateTime(2025, 1, 10), 250.75m, EstadoPedido.Registrado);
}
