using MedicalAppointment.API.DTOs.DoctorDayOff;
using MedicalAppointment.API.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalAppointment.API.Controllers;

[ApiController]
[Route("api/doctors/{doctorId:int}/day-offs")]
public class DoctorDayOffsController(IDoctorDayOffService dayOffService) : ControllerBase
{
    private readonly IDoctorDayOffService _dayOffService = dayOffService;

    /// <summary>
    /// GET /api/doctors/{doctorId}/day-offs?from=yyyy-MM-dd&amp;to=yyyy-MM-dd
    /// Lấy danh sách ngày nghỉ của bác sĩ, sắp xếp ngày tăng dần. Công khai.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDayOffs(int doctorId, [FromQuery] DoctorDayOffQueryParameters query)
    {
        var result = await _dayOffService.GetDayOffsByDoctorIdAsync(doctorId, query);
        return Ok(result);
    }

    /// <summary>
    /// POST /api/doctors/{doctorId}/day-offs
    /// Thêm ngày nghỉ cho bác sĩ. ADMIN only.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> CreateDayOff(int doctorId, CreateDoctorDayOffRequest request)
    {
        var result = await _dayOffService.CreateDayOffAsync(doctorId, request);
        return StatusCode(201, result);
    }

    /// <summary>
    /// DELETE /api/doctors/{doctorId}/day-offs/{id}
    /// Xóa ngày nghỉ của bác sĩ. ADMIN only.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> DeleteDayOff(int doctorId, int id)
    {
        await _dayOffService.DeleteDayOffAsync(doctorId, id);
        return NoContent();
    }
}
