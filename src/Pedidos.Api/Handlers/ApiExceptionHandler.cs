using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Pedidos.Application.Common.Exceptions;
using Pedidos.Domain.Exceptions;

namespace Pedidos.Api.Handlers;

/// <summary>
/// Manejador global de excepciones: traduce los errores de negocio a ProblemDetails
/// (RFC 7807) sin exponer stack traces al cliente.
/// </summary>
public sealed class ApiExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<ApiExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, titulo, detalle, errores) = Traducir(exception);

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Error no controlado procesando {Path}", httpContext.Request.Path);
        }
        else
        {
            _logger.LogWarning(exception, "Error de negocio procesando {Path}", httpContext.Request.Path);
        }

        var problem = new ProblemDetails
        {
            Type = ProblemDetailsBuilder.TipoPorStatus(statusCode),
            Status = statusCode,
            Title = titulo,
            Detail = detalle,
            Instance = httpContext.Request.Path,
        };

        if (errores is not null)
        {
            problem.Extensions["errors"] = ProblemDetailsBuilder.NormalizarErrores(errores);
        }

        httpContext.Response.StatusCode = statusCode;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception,
        });
    }

    private static (int StatusCode, string Titulo, string Detalle, IDictionary<string, string[]>? Errores) Traducir(Exception exception)
    {
        switch (exception)
        {
            case ValidationAppException validacion:
                return (
                    StatusCodes.Status400BadRequest,
                    "Uno o más errores de validación se han producido.",
                    validacion.Message,
                    validacion.Errors);

            case PedidoInvalidoException pedidoInvalido:
                return (
                    StatusCodes.Status400BadRequest,
                    "Datos de pedido no válidos.",
                    pedidoInvalido.Message,
                    new Dictionary<string, string[]> { [pedidoInvalido.Campo] = [pedidoInvalido.Message] });

            case UsuarioInvalidoException usuarioInvalido:
                return (
                    StatusCodes.Status400BadRequest,
                    "Datos de usuario no válidos.",
                    usuarioInvalido.Message,
                    new Dictionary<string, string[]> { [usuarioInvalido.Campo] = [usuarioInvalido.Message] });

            case UnauthorizedException noAutorizado:
                return (StatusCodes.Status401Unauthorized, "No autenticado.", noAutorizado.Message, null);

            case NotFoundException noEncontrado:
                return (StatusCodes.Status404NotFound, "Recurso no encontrado.", noEncontrado.Message, null);

            case ConflictException conflicto:
                return (StatusCodes.Status409Conflict, "Conflicto con el estado actual.", conflicto.Message, null);

            default:
                return (
                    StatusCodes.Status500InternalServerError,
                    "Se ha producido un error inesperado.",
                    "Contacte con el administrador si el problema persiste.",
                    null);
        }
    }
}
