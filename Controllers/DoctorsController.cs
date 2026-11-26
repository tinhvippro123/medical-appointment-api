using MedicalAppointment.API.DTOs.Doctor;
using MedicalAppointment.API.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalAppointment.API.Controllers;

[ApiController]
[Route("api/doctors")]
public class DoctorsController(IDoctorService doctorService) : ControllerBase
{
    private readonly IDoctorService _doctorService = doctorService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] DoctorQueryParameters query)
    {
        var result = await _doctorService.GetAllAsync(query);
        return Ok(result);
    }

    // Đặt route tĩnh "search" tách biệt khỏi "{id:int}"
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] DoctorSearchQueryParameters query)
    {
        var result = await _doctorService.SearchAsync(query);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _doctorService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpGet("{id:int}/related")]
    public async Task<IActionResult> GetRelated(int id, [FromQuery] RelatedDoctorQueryParameters query)
    {
        var result = await _doctorService.GetRelatedAsync(id, query);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Create([FromForm] CreateDoctorRequest request)
    {
        var result = await _doctorService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateDoctorRequest request)
    {
        var result = await _doctorService.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Delete(int id)
    {
        await _doctorService.DeleteAsync(id);
        return NoContent();
    }
}
