using MedicalAppointment.API.DTOs.Doctor;
using MedicalAppointment.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalAppointment.API.Controllers;

[ApiController]
[Route("api/doctors")]
public class DoctorsController(IDoctorService doctorService) : ControllerBase
{
    private readonly IDoctorService _doctorService = doctorService;

    /// <summary>
    /// GET /api/doctors?page=1&amp;pageSize=10
    /// Danh sách bác sĩ đang hoạt động, phân trang. Public.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] DoctorQueryParameters query)
    {
        var result = await _doctorService.GetAllAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/doctors/{id}
    /// Chi tiết 1 bác sĩ (IsActive = false → 404). Public.
    /// Dùng ràng buộc kiểu {id:int} để tránh xung đột với route "search".
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _doctorService.GetByIdAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/doctors/{id}/related?page=1&amp;pageSize=5
    /// Danh sách bác sĩ cùng chuyên khoa (cùng DepartmentId, Id != id, IsActive = true). Public.
    /// Bác sĩ gốc không tồn tại hoặc IsActive = false → 404 DOCTOR_NOT_FOUND.
    /// </summary>
    [HttpGet("{id:int}/related")]
    public async Task<IActionResult> GetRelated(int id, [FromQuery] RelatedDoctorQueryParameters query)
    {
        var result = await _doctorService.GetRelatedAsync(id, query);
        return Ok(result);
    }

    /// <summary>
    /// POST /api/doctors
    /// Tạo bác sĩ mới. ADMIN only. Dùng multipart/form-data ([FromForm]).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Create([FromForm] CreateDoctorRequest request)
    {
        var result = await _doctorService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// PUT /api/doctors/{id}
    /// Cập nhật bác sĩ. ADMIN only. Dùng multipart/form-data ([FromForm]).
    /// PUT thay thế toàn bộ trường; riêng Image: không gửi thì giữ ảnh cũ.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateDoctorRequest request)
    {
        var result = await _doctorService.UpdateAsync(id, request);
        return Ok(result);
    }

    /// <summary>
    /// DELETE /api/doctors/{id}
    /// Xóa mềm bác sĩ (IsActive = false). ADMIN only.
    /// Chặn xóa khi bác sĩ đã có trong OrderDetails.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Delete(int id)
    {
        await _doctorService.DeleteAsync(id);
        return NoContent();
    }
}
