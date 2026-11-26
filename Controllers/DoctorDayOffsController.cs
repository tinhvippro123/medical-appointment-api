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

    [HttpGet]
    public async Task<IActionResult> GetDayOffs(int doctorId, [FromQuery] DoctorDayOffQueryParameters query)
    {
        var result = await _dayOffService.GetDayOffsByDoctorIdAsync(doctorId, query);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> CreateDayOff(int doctorId, CreateDoctorDayOffRequest request)
    {
        var result = await _dayOffService.CreateDayOffAsync(doctorId, request);
        return StatusCode(201, result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> DeleteDayOff(int doctorId, int id)
    {
        await _dayOffService.DeleteDayOffAsync(doctorId, id);
        return NoContent();
    }
}
