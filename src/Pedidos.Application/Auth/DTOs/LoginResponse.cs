namespace Pedidos.Application.Auth.DTOs;

/// <summary>
/// Respuesta de un inicio de sesión correcto.
/// </summary>
public sealed record LoginResponse(string Token, int ExpiresIn);
