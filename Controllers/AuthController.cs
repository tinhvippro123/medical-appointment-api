using MedicalAppointment.API.DTOs;
using MedicalAppointment.API.Services.Interface;
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
}