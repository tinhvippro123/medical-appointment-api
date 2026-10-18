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
    public async Task<IActionResult> Create(CreateDepartmentDto dto)
    {
        var department = await _departmentService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = department.Id }, department);
    }

    // PUT /api/departments/{id} — Sửa 
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id,UpdateDepartmentDto dto)
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

    // POST /api/departments/{id}/image — Upload ảnh chuyên khoa
    [HttpPost("{id}/image")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> UploadImage(int id, [FromForm] IFormFile file)
    {
        var department = await _departmentService.UploadImageAsync(id, file);
        return Ok(department);
    }

    // Gallery và camera gửi file vào endpoint trên; URL gửi JSON vào endpoint này.
    [HttpPost("{id}/image/url")]
    public async Task<IActionResult> SetImageUrl(int id, [FromBody] DepartmentImageUrlDto dto)
    {
        var department = await _departmentService.SetImageUrlAsync(id, dto.ImageUrl);
        return Ok(department);
    }

    // DELETE /api/departments/{id}/image — Xóa ảnh chuyên khoa
    [HttpDelete("{id}/image")]
    public async Task<IActionResult> DeleteImage(int id)
    {
        var department = await _departmentService.DeleteImageAsync(id);
        return Ok(department);
    }
}

