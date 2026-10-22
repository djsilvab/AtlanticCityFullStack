namespace Pedidos.Application.Auth.Interfaces;

/// <summary>
/// Puerto de hash y verificación de contraseñas. La implementación vive en Infrastructure.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string hash, string password);
}
