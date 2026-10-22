using FluentValidation;
using Pedidos.Application.Pedidos.DTOs;
using Pedidos.Domain.Entities;

namespace Pedidos.Application.Pedidos.Validators;

/// <summary>
/// Reglas de validación de la actualización de un pedido (RF-02 y RF-03).
/// </summary>
public sealed class ActualizarPedidoRequestValidator : AbstractValidator<ActualizarPedidoRequest>
{
    public ActualizarPedidoRequestValidator()
    {
        RuleFor(x => x.NumeroPedido)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El número de pedido es obligatorio.")
            .MaximumLength(Pedido.NumeroPedidoMaxLength)
                .WithMessage($"El número de pedido no puede superar {Pedido.NumeroPedidoMaxLength} caracteres.");

        RuleFor(x => x.Cliente)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El cliente es obligatorio.")
            .MaximumLength(Pedido.ClienteMaxLength)
                .WithMessage($"El cliente no puede superar {Pedido.ClienteMaxLength} caracteres.");

        RuleFor(x => x.Fecha)
            .Cascade(CascadeMode.Stop)
            .NotEqual(default(DateTime)).WithMessage("La fecha del pedido no es válida.");

        RuleFor(x => x.Total)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0m).WithMessage("El total debe ser mayor que cero.")
            .Must(CrearPedidoRequestValidator.TieneComoMaximoDosDecimales)
                .WithMessage("El total admite como máximo 2 decimales.");

        RuleFor(x => x.Estado)
            .Cascade(CascadeMode.Stop)
            .IsInEnum().WithMessage("El estado del pedido no es válido.");
    }
}
