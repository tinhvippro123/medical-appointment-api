using MedicalAppointment.API.DTOs.DoctorSchedule;
using MedicalAppointment.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalAppointment.API.Controllers;

[ApiController]
public class DoctorSchedulesController(IDoctorScheduleService scheduleService) : ControllerBase
{
    private readonly IDoctorScheduleService _scheduleService = scheduleService;

    [HttpGet("api/doctors/{id}/schedules")]
    public async Task<IActionResult> GetSchedulesByDoctorId(int id)
    {
        var schedules = await _scheduleService.GetSchedulesByDoctorIdAsync(id);
        return Ok(schedules);
    }

    [HttpPost("api/doctors/{id}/schedules")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> CreateSchedule(int id, [FromBody] CreateDoctorScheduleRequest request)
    {
        var schedule = await _scheduleService.CreateScheduleAsync(id, request);
        return StatusCode(201, schedule);
    }

    [HttpPut("api/schedules/{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> UpdateSchedule(int id, [FromBody] UpdateDoctorScheduleRequest request)
    {
        var schedule = await _scheduleService.UpdateScheduleAsync(id, request);
        return Ok(schedule);
    }

    [HttpDelete("api/schedules/{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> DeleteSchedule(int id)
    {
        await _scheduleService.DeleteScheduleAsync(id);
        return NoContent();
    }
}
