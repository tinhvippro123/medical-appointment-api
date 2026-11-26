using System.Security.Claims;
using MedicalAppointment.API.DTOs.Payment;
using MedicalAppointment.API.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalAppointment.API.Controllers;

[Route("api/payments")]
[ApiController]
public class PaymentsController(
    IPaymentService paymentService,
    IHostEnvironment hostEnvironment) : ControllerBase
{
    private int? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        return claim != null && int.TryParse(claim.Value, out var userId) ? userId : null;
    }

    private bool IsAdminUser() => User.IsInRole("ADMIN");

    [Authorize]
    [HttpPost("vnpay")]
    public async Task<IActionResult> CreateVnPay(CreatePaymentRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        var baseCallbackUrl = $"{Request.Scheme}://{Request.Host}/api/payments/callback";

        var result = await paymentService.CreateVnPayUrlAsync(
            userId.Value,
            IsAdminUser(),
            request.OrderId,
            clientIp,
            baseCallbackUrl);

        return Ok(result);
    }

    [Authorize]
    [HttpPost("vietqr")]
    public async Task<IActionResult> CreateVietQr(CreatePaymentRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await paymentService.CreateVietQrAsync(
            userId.Value,
            IsAdminUser(),
            request.OrderId);

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("callback")]
    public async Task<IActionResult> Callback()
    {
        var result = await paymentService.HandleVnPayCallbackAsync(Request.Query);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm(ConfirmPaymentRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        // Chỉ cho phép xác nhận mô phỏng trong môi trường Development hoặc bởi ADMIN đối soát
        if (!hostEnvironment.IsDevelopment() && !IsAdminUser())
        {
            return Forbid();
        }

        var result = await paymentService.ConfirmPaymentAsync(
            userId.Value,
            IsAdminUser(),
            request);

        return Ok(result);
    }
}
