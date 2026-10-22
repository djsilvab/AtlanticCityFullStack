namespace Pedidos.Application.Auth.DTOs;

/// <summary>
/// Credenciales de inicio de sesión.
/// </summary>
public sealed record LoginRequest(string Email, string Password);
