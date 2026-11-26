using System.Security.Claims;
using MedicalAppointment.API.DTOs;
using MedicalAppointment.API.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace MedicalAppointment.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    private int? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        return claim != null && int.TryParse(claim.Value, out var userId) ? userId : null;
    }

    [EnableRateLimiting("auth")]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var response = await authService.Register(request);
        return Ok(response);
    }

    [EnableRateLimiting("auth")]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var token = await authService.Login(request);
        return Ok(new { Token = token, Message = "Đăng nhập thành công." });
    }

    [Authorize]
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }
        await authService.ChangePassword(userId.Value, request);
        return Ok(new { message = "Đổi mật khẩu thành công!" });
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(new { message = "Đăng xuất thành công." });
    }

    [EnableRateLimiting("auth")]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        var devOtp = await authService.ForgotPassword(request);
        if (!string.IsNullOrEmpty(devOtp))
        {
            return Ok(new
            {
                message = "Mã OTP đặt lại mật khẩu đã được gửi tới email của bạn.",
                devOtp
            });
        }

        return Ok(new { message = "Mã OTP đặt lại mật khẩu đã được gửi tới email của bạn." });
    }

    [EnableRateLimiting("auth")]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        await authService.ResetPassword(request);
        return Ok(new { message = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập bằng mật khẩu mới." });
    }

    [EnableRateLimiting("auth")]
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(VerifyEmailRequest request)
    {
        await authService.VerifyEmail(request);
        return Ok(new { message = "Xác thực email thành công! Tài khoản của bạn đã được kích hoạt." });
    }

    [EnableRateLimiting("auth")]
    [HttpPost("resend-verify-email")]
    public async Task<IActionResult> ResendVerifyEmail(ResendVerificationRequest request)
    {
        var devOtp = await authService.ResendVerificationOtp(request);
        if (!string.IsNullOrEmpty(devOtp))
        {
            return Ok(new
            {
                message = "Đã gửi lại mã OTP xác thực tới email của bạn.",
                devOtp
            });
        }

        return Ok(new { message = "Đã gửi lại mã OTP xác thực tới email của bạn." });
    }

    [Authorize]
    [HttpPost("verify-password")]
    public async Task<IActionResult> VerifyPassword(VerifyPasswordRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        await authService.VerifyCurrentPassword(userId.Value, request.Password);
        return Ok(new { verified = true });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var profile = await authService.GetMyProfile(userId.Value);
        return Ok(profile);
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe(UpdateMyProfileRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await authService.UpdateMyProfile(userId.Value, request);
        return Ok(result);
    }

    [EnableRateLimiting("auth")]
    [HttpPost("social-login")]
    public async Task<IActionResult> SocialLogin(SocialLoginRequest request)
    {
        var result = await authService.SocialLogin(request);
        return Ok(result);
    }
}