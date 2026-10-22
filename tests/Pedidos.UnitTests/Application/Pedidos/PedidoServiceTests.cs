using FluentAssertions;
using NSubstitute;
using Pedidos.Application.Common.Exceptions;
using Pedidos.Application.Common.Interfaces;
using Pedidos.Application.Pedidos.DTOs;
using Pedidos.Application.Pedidos.Interfaces;
using Pedidos.Application.Pedidos.Services;
using Pedidos.Domain.Entities;
using Pedidos.Domain.Enums;
using Pedidos.Domain.Exceptions;

namespace Pedidos.UnitTests.Application.Pedidos;

public sealed class PedidoServiceTests
{
    private readonly IPedidoRepository _pedidos = Substitute.For<IPedidoRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly PedidoService _pedidoService;

    public PedidoServiceTests()
    {
        _pedidoService = new PedidoService(_pedidos, _unitOfWork);
    }

    [Fact]
    public async Task GetById_ConPedidoExistente_DevuelveElDto()
    {
        var pedido = new Pedido("PED-001", "Juan Perez", new DateTime(2025, 1, 10), 250.75m, EstadoPedido.Registrado);
        _pedidos.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(pedido);

        var resultado = await _pedidoService.GetByIdAsync(1);

        resultado.NumeroPedido.Should().Be("PED-001");
        resultado.Total.Should().Be(250.75m);
        resultado.Estado.Should().Be(EstadoPedido.Registrado);
    }

    [Fact]
    public async Task GetById_ConPedidoInexistente_LanzaNotFound()
    {
        _pedidos.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((Pedido?)null);

        var obtener = () => _pedidoService.GetByIdAsync(99);

        (await obtener.Should().ThrowAsync<NotFoundException>())
            .Which.Clave.Should().Be("99");
    }

    [Fact]
    public async Task Create_ConDatosValidos_GuardaElPedidoYDevuelveElDto()
    {
        _pedidos.ExistsNumeroPedidoAsync("PED-001", null, Arg.Any<CancellationToken>()).Returns(false);

        var resultado = await _pedidoService.CreateAsync(
            new CrearPedidoRequest("PED-001", "Juan Perez", new DateTime(2025, 1, 10), 250.75m, EstadoPedido.Registrado));

        resultado.Id.Should().Be(0);
        resultado.NumeroPedido.Should().Be("PED-001");

        await _pedidos.Received(1).AddAsync(Arg.Any<Pedido>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_ConNumeroPedidoDuplicado_LanzaConflict()
    {
        _pedidos.ExistsNumeroPedidoAsync("PED-001", null, Arg.Any<CancellationToken>()).Returns(true);

        var crear = () => _pedidoService.CreateAsync(
            new CrearPedidoRequest("PED-001", "Juan Perez", new DateTime(2025, 1, 10), 250.75m, EstadoPedido.Registrado));

        (await crear.Should().ThrowAsync<ConflictException>())
            .Which.Message.Should().Contain("PED-001");

        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task Create_ConTotalNoPositivo_LanzaLaInvarianteDelDominio()
    {
        _pedidos.ExistsNumeroPedidoAsync("PED-001", null, Arg.Any<CancellationToken>()).Returns(false);

        var crear = () => _pedidoService.CreateAsync(
            new CrearPedidoRequest("PED-001", "Juan Perez", new DateTime(2025, 1, 10), 0m, EstadoPedido.Registrado));

        (await crear.Should().ThrowAsync<PedidoInvalidoException>())
            .Which.Campo.Should().Be("total");
    }

    [Fact]
    public async Task Update_ConPedidoExistente_AplicaLosCambios()
    {
        var pedido = new Pedido("PED-001", "Juan Perez", new DateTime(2025, 1, 10), 250.75m, EstadoPedido.Registrado);
        _pedidos.GetByIdForUpdateAsync(1, Arg.Any<CancellationToken>()).Returns(pedido);
        _pedidos.ExistsNumeroPedidoAsync("PED-002", 1, Arg.Any<CancellationToken>()).Returns(false);

        var resultado = await _pedidoService.UpdateAsync(
            1,
            new ActualizarPedidoRequest("PED-002", "Empresa Norte", new DateTime(2025, 2, 2), 99.90m, EstadoPedido.EnProceso));

        resultado.NumeroPedido.Should().Be("PED-002");
        resultado.Cliente.Should().Be("Empresa Norte");
        resultado.Estado.Should().Be(EstadoPedido.EnProceso);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_ConPedidoInexistente_LanzaNotFound()
    {
        _pedidos.GetByIdForUpdateAsync(99, Arg.Any<CancellationToken>()).Returns((Pedido?)null);

        var actualizar = () => _pedidoService.UpdateAsync(
            99,
            new ActualizarPedidoRequest("PED-001", "Juan Perez", new DateTime(2025, 1, 10), 10m, EstadoPedido.Registrado));

        await actualizar.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Update_HaciaOtroPedidoConElMismoNumero_LanzaConflict()
    {
        var pedido = new Pedido("PED-001", "Juan Perez", new DateTime(2025, 1, 10), 250.75m, EstadoPedido.Registrado);
        _pedidos.GetByIdForUpdateAsync(1, Arg.Any<CancellationToken>()).Returns(pedido);
        _pedidos.ExistsNumeroPedidoAsync("PED-002", 1, Arg.Any<CancellationToken>()).Returns(true);

        var actualizar = () => _pedidoService.UpdateAsync(
            1,
            new ActualizarPedidoRequest("PED-002", "Empresa Norte", new DateTime(2025, 2, 2), 99.90m, EstadoPedido.EnProceso));

        await actualizar.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Delete_MarcaElPedidoComoEliminadoLogicamente()
    {
        var pedido = new Pedido("PED-001", "Juan Perez", new DateTime(2025, 1, 10), 250.75m, EstadoPedido.Registrado);
        _pedidos.GetByIdForUpdateAsync(1, Arg.Any<CancellationToken>()).Returns(pedido);

        await _pedidoService.DeleteAsync(1);

        pedido.EsEliminado.Should().BeTrue();
        pedido.FechaEliminacion.Should().NotBeNull();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_ConPedidoInexistente_LanzaNotFound()
    {
        _pedidos.GetByIdForUpdateAsync(99, Arg.Any<CancellationToken>()).Returns((Pedido?)null);

        var eliminar = () => _pedidoService.DeleteAsync(99);

        await eliminar.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetAll_DevuelveLosPedidosMapeados()
    {
        var pedidos = new List<Pedido>
        {
            new("PED-001", "Juan Perez", new DateTime(2025, 1, 10), 250.75m, EstadoPedido.Registrado),
            new("PED-002", "Empresa Norte", new DateTime(2025, 2, 2), 99.90m, EstadoPedido.EnProceso),
        };

        _pedidos.GetAllAsync(Arg.Any<CancellationToken>()).Returns(pedidos);

        var resultado = await _pedidoService.GetAllAsync();

        resultado.Should().HaveCount(2);
        resultado.Select(p => p.NumeroPedido).Should().ContainInOrder("PED-001", "PED-002");
    }
}
