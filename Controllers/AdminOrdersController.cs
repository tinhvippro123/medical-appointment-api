using MedicalAppointment.API.DTOs.Order;
using MedicalAppointment.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalAppointment.API.Controllers;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = "ADMIN")]
public class AdminOrdersController(IAdminOrderService adminOrderService) : ControllerBase
{
    private readonly IAdminOrderService _adminOrderService = adminOrderService;

    /// <summary>
    /// GET /api/admin/orders
    /// Danh sách tất cả đơn đặt lịch, sắp xếp CreatedAt DESC, phân trang PagedResult&lt;T&gt;,
    /// lọc tùy chọn theo Status, Keyword (tên/SĐT/mã đơn) và khoảng ngày FromDate - ToDate.
    /// ADMIN only.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] AdminOrderQueryParameters query)
    {
        var result = await _adminOrderService.GetAllOrdersAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/admin/orders/{id:int}
    /// Chi tiết 1 đơn đặt lịch kèm OrderDetails.
    /// ADMIN only.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _adminOrderService.GetOrderByIdAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// PUT /api/admin/orders/{id:int}/status
    /// Cập nhật trạng thái đơn đặt lịch và đồng bộ chi tiết.
    /// ADMIN only.
    /// </summary>
    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateOrderStatusRequest request)
    {
        var result = await _adminOrderService.UpdateOrderStatusAsync(id, request);
        return Ok(result);
    }
}
