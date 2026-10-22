using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pedidos.Application.Pedidos.DTOs;
using Pedidos.Application.Pedidos.Services;
using Pedidos.Domain.Enums;

namespace Pedidos.Api.Controllers;

/// <summary>
/// CRUD de pedidos. Todos los endpoints requieren autenticación; el borrado
/// es exclusivo del rol <c>Admin</c> (decisión D6).
/// </summary>
[ApiController]
[Route("api/pedidos")]
[Authorize]
public sealed class PedidosController : ControllerBase
{
    private readonly PedidoService _pedidoService;

    public PedidosController(PedidoService pedidoService)
    {
        _pedidoService = pedidoService;
    }

    /// <summary>
    /// Lista los pedidos no eliminados, de más reciente a más antiguo.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PedidoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<PedidoDto>>> GetAll(CancellationToken cancellationToken)
    {
        var pedidos = await _pedidoService.GetAllAsync(cancellationToken);

        return Ok(pedidos);
    }

    /// <summary>
    /// Lista paginada y filtrable de pedidos (filtro por estado y búsqueda por cliente).
    /// </summary>
    [HttpGet("paginado")]
    [ProducesResponseType(typeof(PaginaPedidos), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PaginaPedidos>> GetPage(
        [FromQuery] int pagina = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] EstadoPedido? estado = null,
        [FromQuery] string? cliente = null,
        CancellationToken cancellationToken = default)
    {
        var consulta = new PedidoConsulta(pagina, pageSize, estado, cliente);

        return Ok(await _pedidoService.GetPageAsync(consulta, cancellationToken));
    }

    /// <summary>
    /// Obtiene un pedido por su identificador.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PedidoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PedidoDto>> GetById(int id, CancellationToken cancellationToken)
    {
        return Ok(await _pedidoService.GetByIdAsync(id, cancellationToken));
    }

    /// <summary>
    /// Crea un pedido.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PedidoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PedidoDto>> Create(
        [FromBody] CrearPedidoRequest request,
        CancellationToken cancellationToken)
    {
        var pedido = await _pedidoService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = pedido.Id }, pedido);
    }

    /// <summary>
    /// Actualiza un pedido. El identificador de la ruta manda sobre el del cuerpo.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(PedidoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PedidoDto>> Update(
        int id,
        [FromBody] ActualizarPedidoRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _pedidoService.UpdateAsync(id, request, cancellationToken));
    }

    /// <summary>
    /// Elimina un pedido de forma lógica. Solo disponible para el rol <c>Admin</c>.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = RolNombre.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _pedidoService.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}
