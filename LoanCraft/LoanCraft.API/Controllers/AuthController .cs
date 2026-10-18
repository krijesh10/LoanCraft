using Microsoft.AspNetCore.Mvc;
using LoanCraft.Application.Common.DTOs;
using LoanCraft.Application.Common.Interfaces;

namespace LoanCraft.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        return Ok(result);
    }

    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                 ?? User.FindFirst("email")?.Value;

        if (string.IsNullOrEmpty(email)) return Unauthorized();

        var result = await _authService.RefreshTokenAsync(email);
        return Ok(result);
    }

}
