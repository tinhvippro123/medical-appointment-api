using System.Security.Claims;
using MedicalAppointment.API.DTOs.User;
using MedicalAppointment.API.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalAppointment.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "ADMIN")]
public class UsersController(IUserService userService) : ControllerBase
{
    private int? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        return claim != null && int.TryParse(claim.Value, out var userId) ? userId : null;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] UserQueryParameters query)
    {
        var result = await userService.GetAllAsync(query);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await userService.GetByIdAsync(id);
        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequest request)
    {
        var user = await userService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateUserRequest request)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized();

        var user = await userService.UpdateAsync(id, currentUserId.Value, request);
        return Ok(user);
    }

    [HttpPut("{id:int}/role")]
    public async Task<IActionResult> UpdateRole(int id, UpdateUserRoleRequest request)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized();

        var user = await userService.UpdateRoleAsync(id, currentUserId.Value, request);
        return Ok(user);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateUserStatusRequest request)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized();

        var user = await userService.UpdateStatusAsync(id, currentUserId.Value, request);
        return Ok(user);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized();

        await userService.DeleteAsync(id, currentUserId.Value);
        return NoContent();
    }
}
