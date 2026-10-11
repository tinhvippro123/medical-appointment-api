using System.Security.Claims;
using MedicalAppointment.API.DTOs;
using MedicalAppointment.API.Services;
using MedicalAppointment.API.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace MedicalAppointment.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        await authService.Register(request);
        return Ok(new { Message = "Đăng ký thành công." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var token = await authService.Login(request);
        return Ok(new { Token = token, Message = "Đăng nhập thành công." });
    }

    [Authorize]
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId)){
            return Unauthorized();
        }
        await authService.ChangePassword(userId, request);
        return Ok(new{message = "Đổi mật khẩu thành công!"});
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(new{message = "Đăng xuất thành công."});
    }
}