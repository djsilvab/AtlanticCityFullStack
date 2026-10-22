using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Pedidos.Api.Handlers;

namespace Pedidos.Api.Filters;

/// <summary>
/// Ejecuta los validadores de FluentValidation sobre el cuerpo de la petición.
/// Si hay errores devuelve 400 con el detalle por campo (RFC 7807).
/// </summary>
public sealed class ValidacionFluentValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var peticion = context.ActionArguments.Values.FirstOrDefault();

        if (peticion is not null)
        {
            var error = await ValidarAsync(context, peticion);

            if (error is not null)
            {
                context.Result = error;
                return;
            }
        }

        await next();
    }

    private static async Task<ActionResult?> ValidarAsync(ActionExecutingContext context, object peticion)
    {
        var tipoValidador = typeof(IValidator<>).MakeGenericType(peticion.GetType());
        var validador = context.HttpContext.RequestServices.GetService(tipoValidador);

        if (validador is null)
        {
            return null;
        }

        var resultado = await ((IValidator)validador).ValidateAsync(
            new ValidationContext<object>(peticion),
            context.HttpContext.RequestAborted);

        if (resultado.IsValid)
        {
            return null;
        }

        var errores = ProblemDetailsBuilder.NormalizarErrores(resultado.Errors
            .GroupBy(error => error.PropertyName, StringComparer.Ordinal)
            .ToDictionary(
                grupo => grupo.Key,
                grupo => grupo.Select(error => error.ErrorMessage).ToArray(),
                StringComparer.Ordinal));

        var problem = new ValidationProblemDetails(errores)
        {
            Type = ProblemDetailsBuilder.TipoPorStatus(StatusCodes.Status400BadRequest),
            Status = StatusCodes.Status400BadRequest,
            Title = "Uno o más errores de validación se han producido.",
            Detail = "Revisa los campos indicados e inténtalo de nuevo.",
            Instance = context.HttpContext.Request.Path,
        };

        problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

        return new BadRequestObjectResult(problem);
    }
}
