using MedicalAppointment.API.DTOs.Role;
using MedicalAppointment.API.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalAppointment.API.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize(Roles = "ADMIN")]
public class RolesController(IRoleService roleService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var roles = await roleService.GetAllAsync();
        return Ok(roles);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var role = await roleService.GetByIdAsync(id);
        return Ok(role);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateRoleRequest request)
    {
        var role = await roleService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateRoleRequest request)
    {
        var role = await roleService.UpdateAsync(id, request);
        return Ok(role);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await roleService.DeleteAsync(id);
        return NoContent();
    }
}
