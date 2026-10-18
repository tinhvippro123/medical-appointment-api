using MedicalAppointment.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalAppointment.API.Controllers;

[ApiController]
[Route("api/doctors")]
public class DoctorController(ApplicationDbContext context) : ControllerBase
{
    private IQueryable<MedicalAppointment.API.Entities.Doctor> VisibleDoctors() =>
        context.Doctors.AsNoTracking().Where(d => d.IsActive && d.Department.IsActive);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] int? departmentId = null, [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100 || (long)(page - 1) * pageSize > int.MaxValue)
            return BadRequest(new { message = "Phân trang không hợp lệ." });
        var query = VisibleDoctors();
        if (departmentId.HasValue) query = query.Where(d => d.DepartmentId == departmentId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(d => d.FullName.Contains(term) || d.Department.Name.Contains(term));
        }
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(d => d.CreatedAt).ThenByDescending(d => d.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(d => new { d.Id, d.FullName, d.Qualification, d.DepartmentId,
                DepartmentName = d.Department.Name, d.AvatarUrl, d.Description,
                d.ExperienceYears, d.ConsultationFee }).ToListAsync(cancellationToken);
        return Ok(new { items, page, pageSize, totalCount, hasNextPage = (long)page * pageSize < totalCount });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var doctor = await VisibleDoctors().Where(d => d.Id == id)
            .Select(d => new { d.Id, d.FullName, d.Qualification, d.DepartmentId,
                DepartmentName = d.Department.Name, d.AvatarUrl, d.Description,
                d.ExperienceYears, d.ConsultationFee }).FirstOrDefaultAsync(cancellationToken);
        return doctor is null ? NotFound() : Ok(doctor);
    }
}
