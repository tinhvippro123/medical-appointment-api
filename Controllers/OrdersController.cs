using System.Security.Claims;
using MedicalAppointment.API.DTOs.Order;
using MedicalAppointment.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalAppointment.API.Controllers;

[Route("api/orders")]
[ApiController]
[Authorize]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    private int? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (claim != null && int.TryParse(claim.Value, out int userId))
        {
            return userId;
        }
        return null;
    }

    /// <summary>
    /// POST /api/orders/checkout
    /// Chuyển toàn bộ giỏ hàng của user thành 1 đơn đặt lịch và xóa sạch giỏ hàng.
    /// </summary>
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(CheckoutRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await orderService.CheckoutAsync(userId.Value, request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// GET /api/orders
    /// Danh sách đơn đặt lịch của user đang đăng nhập, sắp xếp CreatedAt DESC, phân trang.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] OrderQueryParameters query)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await orderService.GetOrdersAsync(userId.Value, query);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/orders/{id:int}
    /// Chi tiết đơn đặt lịch kèm OrderDetails của user đang đăng nhập.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await orderService.GetOrderByIdAsync(userId.Value, id);
        return Ok(result);
    }
}
