using Microsoft.AspNetCore.Mvc;
using MedicalAppointment.API.DTOs.Department;
using MedicalAppointment.API.Services;

namespace MedicalAppointment.API.Controllers;

[ApiController]
[Route("api/departments")]
public class DepartmentController(IDepartmentService departmentService) : ControllerBase
{
    private readonly IDepartmentService _departmentService = departmentService;

    // GET /api/departments — Danh sách chuyên khoa
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var departments = await _departmentService.GetAllAsync();
        return Ok(departments);
    }

    // GET /api/departments/{id} — Chi tiết 1 chuyên khoa
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var department = await _departmentService.GetByIdAsync(id);
        return Ok(department);
    }

    // POST /api/departments — Thêm mới 
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentDto dto)
    {
        var department = await _departmentService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = department.Id }, department);
    }

    // PUT /api/departments/{id} — Sửa 
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDepartmentDto dto)
    {
        var department = await _departmentService.UpdateAsync(id, dto);
        return Ok(department);
    }

    // DELETE /api/departments/{id} — Xóa 
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _departmentService.DeleteAsync(id);
        return NoContent();
    }
}

