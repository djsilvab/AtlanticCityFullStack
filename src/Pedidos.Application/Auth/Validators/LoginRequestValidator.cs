using FluentValidation;
using Pedidos.Application.Auth.DTOs;

namespace Pedidos.Application.Auth.Validators;

/// <summary>
/// Reglas de validación del inicio de sesión (RF-01).
/// </summary>
public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public const int PasswordMinLength = 6;

    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El email es obligatorio.")
            .EmailAddress().WithMessage("El email no tiene un formato válido.");

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(PasswordMinLength)
                .WithMessage($"La contraseña debe tener al menos {PasswordMinLength} caracteres.");
    }
}
