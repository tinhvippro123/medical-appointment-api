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

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] AdminOrderQueryParameters query)
    {
        var result = await _adminOrderService.GetAllOrdersAsync(query);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _adminOrderService.GetOrderByIdAsync(id);
        return Ok(result);
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateOrderStatusRequest request)
    {
        var result = await _adminOrderService.UpdateOrderStatusAsync(id, request);
        return Ok(result);
    }
}
