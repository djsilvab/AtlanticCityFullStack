namespace Pedidos.Application.Common.Exceptions;

/// <summary>
/// Recurso no encontrado. Se traduce a HTTP 404.
/// </summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string recurso, string clave)
        : base($"No se ha encontrado el recurso '{recurso}' con la clave '{clave}'.")
    {
        Recurso = recurso;
        Clave = clave;
    }

    public string Recurso { get; }

    public string Clave { get; }
}
