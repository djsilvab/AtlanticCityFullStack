using Pedidos.Domain.Enums;
using Pedidos.Domain.Exceptions;

namespace Pedidos.Domain.Entities;

/// <summary>
/// Entidad raíz del pedido. Contiene las invariantes del negocio:
/// número de pedido obligatorio y único, cliente obligatorio, total mayor que cero
/// y estado dentro del catálogo válido.
/// </summary>
public sealed class Pedido
{
    public const int NumeroPedidoMaxLength = 50;
    public const int ClienteMaxLength = 150;

    // Constructor para materialización de EF Core.
    private Pedido()
    {
        NumeroPedido = string.Empty;
        Cliente = string.Empty;
    }

    public int Id { get; private set; }

    public string NumeroPedido { get; private set; }

    public string Cliente { get; private set; }

    public DateTime Fecha { get; private set; }

    public decimal Total { get; private set; }

    public EstadoPedido Estado { get; private set; }

    /// <summary>Eliminación lógica: el registro nunca se borra de la base de datos.</summary>
    public bool EsEliminado { get; private set; }

    public DateTime? FechaEliminacion { get; private set; }

    public Pedido(string numeroPedido, string cliente, DateTime fecha, decimal total, EstadoPedido estado)
    {
        NumeroPedido = ValidarNumeroPedido(numeroPedido);
        Cliente = ValidarCliente(cliente);
        Fecha = ValidarFecha(fecha);
        Total = ValidarTotal(total);
        Estado = ValidarEstado(estado);
    }

    /// <summary>Aplica los cambios de una actualización y vuelve a validar las invariantes.</summary>
    public void Actualizar(string numeroPedido, string cliente, DateTime fecha, decimal total, EstadoPedido estado)
    {
        if (EsEliminado)
        {
            throw new PedidoInvalidoException("estado", "no se puede actualizar un pedido eliminado.");
        }

        NumeroPedido = ValidarNumeroPedido(numeroPedido);
        Cliente = ValidarCliente(cliente);
        Fecha = ValidarFecha(fecha);
        Total = ValidarTotal(total);
        Estado = ValidarEstado(estado);
    }

    /// <summary>Marca el pedido como eliminado lógicamente. Es idempotente.</summary>
    public void MarcarComoEliminado(DateTime fechaEliminacion)
    {
        if (EsEliminado)
        {
            return;
        }

        EsEliminado = true;
        FechaEliminacion = fechaEliminacion;
    }

    private static string ValidarNumeroPedido(string numeroPedido)
    {
        if (string.IsNullOrWhiteSpace(numeroPedido))
        {
            throw new PedidoInvalidoException("numeroPedido", "es obligatorio.");
        }

        var valor = numeroPedido.Trim();

        if (valor.Length > NumeroPedidoMaxLength)
        {
            throw new PedidoInvalidoException("numeroPedido", $"no puede superar {NumeroPedidoMaxLength} caracteres.");
        }

        return valor;
    }

    private static string ValidarCliente(string cliente)
    {
        if (string.IsNullOrWhiteSpace(cliente))
        {
            throw new PedidoInvalidoException("cliente", "es obligatorio.");
        }

        var valor = cliente.Trim();

        if (valor.Length > ClienteMaxLength)
        {
            throw new PedidoInvalidoException("cliente", $"no puede superar {ClienteMaxLength} caracteres.");
        }

        return valor;
    }

    private static DateTime ValidarFecha(DateTime fecha)
    {
        if (fecha == default)
        {
            throw new PedidoInvalidoException("fecha", "debe ser una fecha válida.");
        }

        return fecha;
    }

    private static decimal ValidarTotal(decimal total)
    {
        if (total <= 0m)
        {
            throw new PedidoInvalidoException("total", "debe ser mayor que cero.");
        }

        return total;
    }

    private static EstadoPedido ValidarEstado(EstadoPedido estado)
    {
        if (!Enum.IsDefined(estado))
        {
            throw new PedidoInvalidoException("estado", "no es un estado válido.");
        }

        return estado;
    }
}
