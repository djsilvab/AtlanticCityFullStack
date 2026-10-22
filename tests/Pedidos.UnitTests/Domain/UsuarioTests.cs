using FluentAssertions;
using Pedidos.Domain.Entities;
using Pedidos.Domain.Enums;
using Pedidos.Domain.Exceptions;

namespace Pedidos.UnitTests.Domain;

public sealed class UsuarioTests
{
    [Fact]
    public void Constructor_ConValidos_GuardaLosValores()
    {
        var usuario = new Usuario("user@email.com", "hash", Rol.User);

        usuario.Email.Should().Be("user@email.com");
        usuario.PasswordHash.Should().Be("hash");
        usuario.Rol.Should().Be(Rol.User);
        usuario.Activo.Should().BeTrue();
    }

    [Fact]
    public void Constructor_NormalizaElEmail()
    {
        var usuario = new Usuario("  User@Email.COM ", "hash", Rol.Admin);

        usuario.Email.Should().Be("user@email.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_SinEmail_LanzaExcepcion(string email)
    {
        var crear = () => new Usuario(email, "hash", Rol.User);

        crear.Should().Throw<UsuarioInvalidoException>()
            .Which.Campo.Should().Be("email");
    }

    [Theory]
    [InlineData("sin-arroba")]
    [InlineData("con espacio@email.com")]
    public void Constructor_EmailConFormatoInvalido_LanzaExcepcion(string email)
    {
        var crear = () => new Usuario(email, "hash", Rol.User);

        crear.Should().Throw<UsuarioInvalidoException>()
            .Which.Campo.Should().Be("email");
    }

    [Fact]
    public void Constructor_SinPasswordHash_LanzaExcepcion()
    {
        var crear = () => new Usuario("user@email.com", "  ", Rol.User);

        crear.Should().Throw<UsuarioInvalidoException>()
            .Which.Campo.Should().Be("passwordHash");
    }

    [Fact]
    public void Constructor_RolNoDefinido_LanzaExcepcion()
    {
        var crear = () => new Usuario("user@email.com", "hash", (Rol)99);

        crear.Should().Throw<UsuarioInvalidoException>()
            .Which.Campo.Should().Be("rol");
    }

    [Fact]
    public void Desactivar_YPoderActivar_GestionanElEstado()
    {
        var usuario = new Usuario("user@email.com", "hash", Rol.User);

        usuario.Desactivar();
        usuario.Activo.Should().BeFalse();

        usuario.Activar();
        usuario.Activo.Should().BeTrue();
    }

    [Fact]
    public void NormalizarEmail_EliminaEspaciosYPasaAMinusculas()
    {
        Usuario.NormalizarEmail("  Admin@Email.COM  ").Should().Be("admin@email.com");
    }
}
