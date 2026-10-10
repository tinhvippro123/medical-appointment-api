using System.Security.Claims;
using MedicalAppointment.API.DTOs.PatientProfile;
using MedicalAppointment.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalAppointment.API.Controllers;

[Route("api/patient-profiles")]
[ApiController]
[Authorize]
public class PatientProfilesController(IPatientProfileService patientProfileService) : ControllerBase
{
    private readonly IPatientProfileService _patientProfileService = patientProfileService;

    private int? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (claim != null && int.TryParse(claim.Value, out int userId))
        {
            return userId;
        }
        return null;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        int? userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _patientProfileService.GetAllByUserIdAsync(userId.Value);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePatientProfileRequest request)
    {
        int? userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _patientProfileService.CreateAsync(userId.Value, request);
        return CreatedAtAction(nameof(GetAll), null, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePatientProfileRequest request)
    {
        int? userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _patientProfileService.UpdateAsync(id, userId.Value, request);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        int? userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        await _patientProfileService.DeleteAsync(id, userId.Value);
        return NoContent();
    }
}
