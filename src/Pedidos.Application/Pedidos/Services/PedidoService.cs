using Pedidos.Application.Common.Exceptions;
using Pedidos.Application.Common.Interfaces;
using Pedidos.Application.Common.Mapping;
using Pedidos.Application.Pedidos.DTOs;
using Pedidos.Application.Pedidos.Interfaces;
using Pedidos.Domain.Entities;

namespace Pedidos.Application.Pedidos.Services;

/// <summary>
/// Caso de uso del CRUD de pedidos. Contiene las reglas de negocio;
/// los controllers solo traducen HTTP a estos casos de uso.
/// </summary>
public sealed class PedidoService
{
    private readonly IPedidoRepository _pedidos;
    private readonly IUnitOfWork _unitOfWork;

    public PedidoService(IPedidoRepository pedidos, IUnitOfWork unitOfWork)
    {
        _pedidos = pedidos;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<PedidoDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var pedidos = await _pedidos.GetAllAsync(cancellationToken);

        return pedidos.ToDto();
    }

    public Task<PaginaPedidos> GetPageAsync(PedidoConsulta consulta, CancellationToken cancellationToken = default) =>
        _pedidos.GetPageAsync(consulta, cancellationToken);

    public async Task<PedidoDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var pedido = await _pedidos.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Pedido", id.ToString());

        return pedido.ToDto();
    }

    public async Task<PedidoDto> CreateAsync(CrearPedidoRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureNumeroPedidoDisponibleAsync(request.NumeroPedido, null, cancellationToken);

        var pedido = new Pedido(
            request.NumeroPedido,
            request.Cliente,
            request.Fecha,
            request.Total,
            request.Estado);

        await _pedidos.AddAsync(pedido, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return pedido.ToDto();
    }

    public async Task<PedidoDto> UpdateAsync(int id, ActualizarPedidoRequest request, CancellationToken cancellationToken = default)
    {
        var pedido = await _pedidos.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException("Pedido", id.ToString());

        await EnsureNumeroPedidoDisponibleAsync(request.NumeroPedido, id, cancellationToken);

        pedido.Actualizar(
            request.NumeroPedido,
            request.Cliente,
            request.Fecha,
            request.Total,
            request.Estado);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return pedido.ToDto();
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var pedido = await _pedidos.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException("Pedido", id.ToString());

        pedido.MarcarComoEliminado(DateTime.UtcNow);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureNumeroPedidoDisponibleAsync(
        string numeroPedido,
        int? idExcluido,
        CancellationToken cancellationToken)
    {
        var yaExiste = await _pedidos.ExistsNumeroPedidoAsync(numeroPedido.Trim(), idExcluido, cancellationToken);

        if (yaExiste)
        {
            throw new ConflictException($"Ya existe un pedido con el número '{numeroPedido.Trim()}'.");
        }
    }
}
