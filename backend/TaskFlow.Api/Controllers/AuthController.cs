using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Dtos;
using TaskFlow.Api.Extensions;
using TaskFlow.Api.Services;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AuthService authService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] AuthRequest request)
        => Ok(await authService.LoginAsync(request));

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me()
        => Ok(await authService.GetCurrentUserAsync(HttpContext.GetUserId()));
}
