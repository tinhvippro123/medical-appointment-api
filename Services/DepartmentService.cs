using Microsoft.EntityFrameworkCore;
using MedicalAppointment.API.Data;
using MedicalAppointment.API.DTOs.Department;
using MedicalAppointment.API.Entities;
using MedicalAppointment.API.Exceptions;

namespace MedicalAppointment.API.Services;

public class DepartmentService(ApplicationDbContext context) : IDepartmentService
{
    private readonly ApplicationDbContext _context = context;

    // Lấy danh sách chuyên khoa (chỉ IsActive = true)
    public async Task<List<DepartmentDto>> GetAllAsync()
    {
        var departments = await _context.Departments
            .Where(d => d.IsActive)
            .ToListAsync();

        var result = new List<DepartmentDto>();
        foreach (var d in departments)
        {
            result.Add(new DepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                ImageUrl = d.ImageUrl,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            });
        }
        return result;
    }

    // Lấy chi tiết 1 chuyên khoa theo Id
    public async Task<DepartmentDto> GetByIdAsync(int id)
    {
        var d = await _context.Departments.FindAsync(id);
        if (d == null || !d.IsActive)
            throw new NotFoundException(ErrorCode.DEPARTMENT_NOT_FOUND);

        return new DepartmentDto
        {
            Id = d.Id,
            Name = d.Name,
            Description = d.Description,
            ImageUrl = d.ImageUrl,
            IsActive = d.IsActive,
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt
        };
    }

    // Tạo mới chuyên khoa
    public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto)
    {
        var department = await _context.Departments
            .FirstOrDefaultAsync(d => d.Name == dto.Name);

        if (department != null)
        {
            if (department.IsActive)
            {
                // Đã có và đang hoạt động -> lỗi trùng tên
                throw new BadRequestException(ErrorCode.DEPARTMENT_NAME_EXISTS);
            }
            
            // Đã có nhưng bị xóa mềm -> Khôi phục
            department.IsActive = true;
            department.Description = dto.Description;
            department.ImageUrl = dto.ImageUrl;
        }
        else
        {
            // Chưa có -> Tạo mới hoàn toàn
            department = new Department
            {
                Name = dto.Name,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl
            };
            _context.Departments.Add(department);
        }

        await _context.SaveChangesAsync();

        return new DepartmentDto
        {
            Id = department.Id,
            Name = department.Name,
            Description = department.Description,
            ImageUrl = department.ImageUrl,
            IsActive = department.IsActive,
            CreatedAt = department.CreatedAt,
            UpdatedAt = department.UpdatedAt
        };
    }

    // Cập nhật chuyên khoa
    public async Task<DepartmentDto> UpdateAsync(int id, UpdateDepartmentDto dto)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department == null || !department.IsActive)
            throw new NotFoundException(ErrorCode.DEPARTMENT_NOT_FOUND);

        // Kiểm tra trùng tên (trừ chính nó, tính cả những khoa đã xóa mềm để tránh lỗi Unique Index của DB)
        var exists = await _context.Departments
            .AnyAsync(d => d.Name == dto.Name && d.Id != id);
        if (exists)
            throw new BadRequestException(ErrorCode.DEPARTMENT_NAME_EXISTS);

        department.Name = dto.Name;
        department.Description = dto.Description;
        department.ImageUrl = dto.ImageUrl;

        await _context.SaveChangesAsync();

        return new DepartmentDto
        {
            Id = department.Id,
            Name = department.Name,
            Description = department.Description,
            ImageUrl = department.ImageUrl,
            IsActive = department.IsActive,
            CreatedAt = department.CreatedAt,
            UpdatedAt = department.UpdatedAt
        };
    }

    // Xóa mềm chuyên khoa (IsActive = false)
    public async Task DeleteAsync(int id)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department == null || !department.IsActive)
            throw new NotFoundException(ErrorCode.DEPARTMENT_NOT_FOUND);

        // Kiểm tra xem khoa còn bác sĩ nào đang hoạt động không
        var hasDoctors = await _context.Doctors
            .AnyAsync(d => d.DepartmentId == id && d.IsActive);
        if (hasDoctors)
            throw new BadRequestException(ErrorCode.DEPARTMENT_HAS_DOCTORS);

        department.IsActive = false;
        await _context.SaveChangesAsync();
    }
}
