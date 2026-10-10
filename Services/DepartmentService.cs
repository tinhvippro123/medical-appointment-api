using Microsoft.EntityFrameworkCore;
using MedicalAppointment.API.Data;
using MedicalAppointment.API.DTOs.Department;
using MedicalAppointment.API.Entities;
using MedicalAppointment.API.Exceptions;

namespace MedicalAppointment.API.Services;

public class DepartmentService(ApplicationDbContext context, IWebHostEnvironment env) : IDepartmentService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IWebHostEnvironment _env = env;

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

    // Xóa mềm chuyên khoa (IsActive = false) + xóa file ảnh kèm theo
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

        // Xóa file ảnh trên server (nếu có)
        if (department.ImageUrl != null && department.ImageUrl != "")
        {
            var path = department.ImageUrl.TrimStart('/');
            var filePath = Path.Combine(_env.WebRootPath, path);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        department.IsActive = false;
        department.ImageUrl = null;
        await _context.SaveChangesAsync();
    }

    // Upload ảnh mới cho chuyên khoa
    public async Task<DepartmentDto> UploadImageAsync(int id, IFormFile file)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department == null || !department.IsActive)
            throw new NotFoundException(ErrorCode.DEPARTMENT_NOT_FOUND);

        // Validate file: chỉ cho phép .jpg, .jpeg, .png và < 5MB
        var ext = Path.GetExtension(file.FileName).ToLower();
        if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
        {
            throw new BadRequestException(ErrorCode.DEPARTMENT_IMAGE_INVALID);
        }

        if (file.Length > 5 * 1024 * 1024)
        {
            throw new BadRequestException(ErrorCode.DEPARTMENT_IMAGE_INVALID);
        }

        // Nếu đã có ảnh cũ -> xóa file cũ trước
        if (department.ImageUrl != null && department.ImageUrl != "")
        {
            var oldPath = department.ImageUrl.TrimStart('/');
            var oldFilePath = Path.Combine(_env.WebRootPath, oldPath);
            if (File.Exists(oldFilePath))
            {
                File.Delete(oldFilePath);
            }
        }

        // Lưu file mới vào wwwroot/images/departments/{tên_file}
        var folder = Path.Combine(_env.WebRootPath, "images", "departments");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var fileName = DateTime.Now.Ticks.ToString() + ext;
        var filePath = Path.Combine(folder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Cập nhật ImageUrl trong database
        department.ImageUrl = "/images/departments/" + fileName;
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

    // Xóa ảnh của chuyên khoa
    public async Task<DepartmentDto> DeleteImageAsync(int id)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department == null || !department.IsActive)
            throw new NotFoundException(ErrorCode.DEPARTMENT_NOT_FOUND);

        if (department.ImageUrl == null || department.ImageUrl == "")
            throw new BadRequestException(ErrorCode.DEPARTMENT_NO_IMAGE);

        // Xóa file vật lý trên server
        var path = department.ImageUrl.TrimStart('/');
        var filePath = Path.Combine(_env.WebRootPath, path);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        department.ImageUrl = null;
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
}
