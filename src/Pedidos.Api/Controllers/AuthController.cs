using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pedidos.Application.Auth.DTOs;
using Pedidos.Application.Auth.Services;

namespace Pedidos.Api.Controllers;

/// <summary>
/// Autenticación. Único endpoint público de la API.
/// </summary>
[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Inicia sesión y devuelve un token JWT Bearer.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var respuesta = await _authService.LoginAsync(request, cancellationToken);

        return Ok(respuesta);
    }
}
