using Gateway.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly TokenService _tokenService;

    public AuthController(TokenService tokenService)
    {
        _tokenService = tokenService;
    }

    /// <summary>
    /// Authenticate and receive a JWT token.
    /// Default users: admin/admin, operator/operator, viewer/viewer, simulator/simulator
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (!UserStore.ValidateCredentials(request.Username, request.Password))
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }

        var response = _tokenService.GenerateToken(request.Username);
        return Ok(response);
    }
}
