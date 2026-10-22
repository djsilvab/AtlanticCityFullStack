namespace Pedidos.Application.Auth.Models;

/// <summary>
/// Token JWT emitido y su fecha de expiración.
/// </summary>
public sealed record TokenGenerado(string Token, DateTime ExpiresAt);
