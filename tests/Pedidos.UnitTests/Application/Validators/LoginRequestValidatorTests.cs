using FluentAssertions;
using Pedidos.Application.Auth.DTOs;
using Pedidos.Application.Auth.Validators;

namespace Pedidos.UnitTests.Application.Validators;

public sealed class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void CredencialesValidas_NoDevuelveErrores()
    {
        var resultado = _validator.Validate(new LoginRequest("user@email.com", "123456"));

        resultado.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SinEmail_InvalidaElCampoEmail(string email)
    {
        var resultado = _validator.Validate(new LoginRequest(email, "123456"));

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().ContainSingle(e => e.PropertyName == nameof(LoginRequest.Email));
    }

    [Fact]
    public void EmailConFormatoInvalido_InvalidaElCampoEmail()
    {
        var resultado = _validator.Validate(new LoginRequest("no-es-un-email", "123456"));

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().ContainSingle(e => e.PropertyName == nameof(LoginRequest.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SinPassword_InvalidaElCampoPassword(string password)
    {
        var resultado = _validator.Validate(new LoginRequest("user@email.com", password));

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().ContainSingle(e => e.PropertyName == nameof(LoginRequest.Password));
    }

    [Fact]
    public void PasswordDemasiadoCorta_InvalidaElCampoPassword()
    {
        var resultado = _validator.Validate(new LoginRequest("user@email.com", "12345"));

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().ContainSingle(e => e.PropertyName == nameof(LoginRequest.Password));
    }
}
