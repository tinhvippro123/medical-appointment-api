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

        var oldImageUrl = department.ImageUrl;
        department.IsActive = false;
        department.ImageUrl = null;
        await _context.SaveChangesAsync();
        DeleteLocalImage(oldImageUrl);
    }

    // Upload ảnh mới cho chuyên khoa
    public async Task<DepartmentDto> UploadImageAsync(int id, IFormFile file)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department == null || !department.IsActive)
            throw new NotFoundException(ErrorCode.DEPARTMENT_NOT_FOUND);

        // Validate file: chỉ cho phép .jpg, .jpeg, .png và < 5MB
        if (file == null || file.Length == 0 || file.Length > 5 * 1024 * 1024)
            throw new BadRequestException(ErrorCode.DEPARTMENT_IMAGE_INVALID);

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
        {
            throw new BadRequestException(ErrorCode.DEPARTMENT_IMAGE_INVALID);
        }

        // Kiểm tra phần đầu file để tránh nhận file văn bản đổi đuôi thành ảnh.
        var header = new byte[8];
        using (var input = file.OpenReadStream())
        {
            var bytesRead = await input.ReadAtLeastAsync(header, 8, throwOnEndOfStream: false);
            var isJpeg = bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
            var isPng = bytesRead == 8 && header[0] == 137 && header[1] == 80
                && header[2] == 78 && header[3] == 71 && header[4] == 13
                && header[5] == 10 && header[6] == 26 && header[7] == 10;
            if ((ext == ".png" && !isPng) || (ext != ".png" && !isJpeg))
                throw new BadRequestException(ErrorCode.DEPARTMENT_IMAGE_INVALID);
        }

        // Lưu file mới vào wwwroot/images/departments/{tên_file}
        var folder = GetImageFolder();
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var fileName = Guid.NewGuid().ToString("N") + ext;
        var filePath = Path.Combine(folder, fileName);
        var oldImageUrl = department.ImageUrl;
        try
        {
            using (var stream = new FileStream(filePath, FileMode.CreateNew))
            {
                await file.CopyToAsync(stream);
            }
            department.ImageUrl = "/images/departments/" + fileName;
            await _context.SaveChangesAsync();
        }
        catch
        {
            department.ImageUrl = oldImageUrl;
            if (File.Exists(filePath)) File.Delete(filePath);
            throw;
        }

        // Chỉ xóa ảnh cũ sau khi lưu ảnh mới và database thành công.
        DeleteLocalImage(oldImageUrl);

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

        var oldImageUrl = department.ImageUrl;
        department.ImageUrl = null;
        await _context.SaveChangesAsync();
        DeleteLocalImage(oldImageUrl);

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
    // Chỉ lưu URL, không tải ảnh từ website khác về server.
    public async Task<DepartmentDto> SetImageUrlAsync(int id, string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new BadRequestException(ErrorCode.DEPARTMENT_IMAGE_URL_INVALID);

        imageUrl = imageUrl.Trim();
        if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri)
            || (uri.Scheme != "http" && uri.Scheme != "https")
            || string.IsNullOrEmpty(uri.Host) || !string.IsNullOrEmpty(uri.UserInfo))
            throw new BadRequestException(ErrorCode.DEPARTMENT_IMAGE_URL_INVALID);

        var department = await _context.Departments.FindAsync(id);
        if (department == null || !department.IsActive)
            throw new NotFoundException(ErrorCode.DEPARTMENT_NOT_FOUND);

        var oldImageUrl = department.ImageUrl;
        department.ImageUrl = imageUrl;
        await _context.SaveChangesAsync();
        DeleteLocalImage(oldImageUrl);
        return await GetByIdAsync(id);
    }

    private string GetImageFolder()
    {
        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        return Path.Combine(webRoot, "images", "departments");
    }

    private void DeleteLocalImage(string? imageUrl)
    {
        const string prefix = "/images/departments/";
        // URL bên ngoài không phải file trên server. Chỉ xóa file trực tiếp trong thư mục ảnh.
        if (imageUrl == null || !imageUrl.StartsWith(prefix, StringComparison.Ordinal)) return;
        var fileName = imageUrl[prefix.Length..];
        if (string.IsNullOrEmpty(fileName) || fileName.Contains('/') || fileName.Contains('\\')
            || fileName.Contains(':') || fileName == "." || fileName == "..") return;

        var filePath = Path.Combine(GetImageFolder(), fileName);
        if (File.Exists(filePath)) File.Delete(filePath);
    }
}
