using FluentAssertions;
using NSubstitute;
using Pedidos.Application.Auth.DTOs;
using Pedidos.Application.Auth.Interfaces;
using Pedidos.Application.Auth.Models;
using Pedidos.Application.Auth.Services;
using Pedidos.Application.Common.Exceptions;
using Pedidos.Domain.Entities;
using Pedidos.Domain.Enums;

namespace Pedidos.UnitTests.Application.Auth;

public sealed class AuthServiceTests
{
    private readonly IUsuarioRepository _usuarios = Substitute.For<IUsuarioRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _authService = new AuthService(_usuarios, _passwordHasher, _tokenService);
    }

    [Fact]
    public async Task Login_ConCredencialesValidas_DevuelveElTokenYLaExpiracion()
    {
        var usuario = new Usuario("user@email.com", "hash", Rol.User);
        _usuarios.GetByEmailAsync("user@email.com", Arg.Any<CancellationToken>()).Returns(usuario);
        _passwordHasher.Verify("hash", "123456").Returns(true);
        _tokenService.CrearToken(usuario).Returns(new TokenGenerado("token-jwt", DateTime.UtcNow.AddMinutes(60)));

        var respuesta = await _authService.LoginAsync(new LoginRequest("user@email.com", "123456"));

        respuesta.Token.Should().Be("token-jwt");
        respuesta.ExpiresIn.Should().BeGreaterThan(0).And.BeLessThanOrEqualTo(3600);
    }

    [Fact]
    public async Task Login_ConPasswordIncorrecta_LanzaUnauthorizedConMensajeGenerico()
    {
        var usuario = new Usuario("user@email.com", "hash", Rol.User);
        _usuarios.GetByEmailAsync("user@email.com", Arg.Any<CancellationToken>()).Returns(usuario);
        _passwordHasher.Verify("hash", "incorrecta").Returns(false);

        var login = () => _authService.LoginAsync(new LoginRequest("user@email.com", "incorrecta"));

        (await login.Should().ThrowAsync<UnauthorizedException>())
            .Which.Message.Should().Be(AuthService.MensajeCredencialesInvalidas);
    }

    [Fact]
    public async Task Login_ConUsuarioInexistente_LanzaUnauthorizedSinEmitirToken()
    {
        _usuarios.GetByEmailAsync("nadie@email.com", Arg.Any<CancellationToken>()).Returns((Usuario?)null);

        var login = () => _authService.LoginAsync(new LoginRequest("nadie@email.com", "123456"));

        (await login.Should().ThrowAsync<UnauthorizedException>())
            .Which.Message.Should().Be(AuthService.MensajeCredencialesInvalidas);

        _tokenService.DidNotReceiveWithAnyArgs().CrearToken(default!);
    }

    [Fact]
    public async Task Login_ConUsuarioInactivo_LanzaUnauthorized()
    {
        var usuario = new Usuario("inactivo@email.com", "hash", Rol.User, activo: false);
        _usuarios.GetByEmailAsync("inactivo@email.com", Arg.Any<CancellationToken>()).Returns(usuario);

        var login = () => _authService.LoginAsync(new LoginRequest("inactivo@email.com", "123456"));

        (await login.Should().ThrowAsync<UnauthorizedException>())
            .Which.Message.Should().Be(AuthService.MensajeCredencialesInvalidas);
    }

    [Fact]
    public async Task Login_NoDevuelveElHashNiElPasswordEnLaRespuesta()
    {
        var usuario = new Usuario("user@email.com", "hash-secreto", Rol.User);
        _usuarios.GetByEmailAsync("user@email.com", Arg.Any<CancellationToken>()).Returns(usuario);
        _passwordHasher.Verify("hash-secreto", "123456").Returns(true);
        _tokenService.CrearToken(usuario).Returns(new TokenGenerado("token-jwt", DateTime.UtcNow.AddMinutes(60)));

        var respuesta = await _authService.LoginAsync(new LoginRequest("user@email.com", "123456"));

        respuesta.Token.Should().NotContain("hash-secreto");
        respuesta.ExpiresIn.Should().BePositive();
    }
}
