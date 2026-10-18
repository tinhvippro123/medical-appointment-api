using MedicalAppointment.API.Data;
using MedicalAppointment.API.DTOs.Common;
using MedicalAppointment.API.DTOs.Doctor;
using MedicalAppointment.API.Entities;
using MedicalAppointment.API.Exceptions;
using MedicalAppointment.API.Helpers;
using Microsoft.EntityFrameworkCore;

namespace MedicalAppointment.API.Services;

public class DoctorService : IDoctorService
{
    private readonly ApplicationDbContext _context;
    private readonly ImageStorageHelper _imageHelper;

    public DoctorService(ApplicationDbContext context, ImageStorageHelper imageHelper)
    {
        _context = context;
        _imageHelper = imageHelper;
    }

    public async Task<PagedResult<DoctorResponse>> GetAllAsync(DoctorQueryParameters query)
    {
        var baseQuery = _context.Doctors
            .Where(d => d.IsActive)
            .Include(d => d.Department)
            .Include(d => d.Room)
            .OrderBy(d => d.Id)
            .AsNoTracking();

        var totalCount = await baseQuery.CountAsync();

        var doctorEntities = await baseQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var doctors = doctorEntities.Select(MapToResponse).ToList();

        return new PagedResult<DoctorResponse>
        {
            Items = doctors,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<DoctorResponse> GetByIdAsync(int id)
    {
        var doctor = await _context.Doctors
            .Include(d => d.Department)
            .Include(d => d.Room)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);

        if (doctor == null || !doctor.IsActive)
        {
            throw new NotFoundException(ErrorCode.DOCTOR_NOT_FOUND);
        }

        return MapToResponse(doctor);
    }

    public async Task<DoctorResponse> CreateAsync(CreateDoctorRequest request)
    {
        // Validate DepartmentId tồn tại và còn hoạt động
        var department = await _context.Departments.FindAsync(request.DepartmentId!.Value);
        if (department == null || !department.IsActive)
        {
            throw new BadRequestException(ErrorCode.DEPARTMENT_NOT_FOUND);
        }

        // Validate RoomId (nếu có)
        Room? room = null;
        if (request.RoomId.HasValue)
        {
            room = await _context.Rooms.FindAsync(request.RoomId.Value);
            if (room == null || !room.IsActive)
            {
                throw new BadRequestException(ErrorCode.ROOM_NOT_FOUND);
            }
            if (room.DepartmentId != request.DepartmentId!.Value)
            {
                throw new BadRequestException(ErrorCode.ROOM_NOT_IN_DEPARTMENT);
            }
        }

        // Validate Email trùng (chỉ khi Email không null/rỗng)
        await ValidateEmailUniqueAsync(request.Email, excludeDoctorId: null);

        // Lưu ảnh (nếu có)
        string? avatarUrl = null;
        if (request.Image != null)
        {
            avatarUrl = await _imageHelper.SaveImageAsync(request.Image, "doctors");
        }

        var doctor = new Entities.Doctor
        {
            FullName = request.FullName.Trim(),
            Qualification = request.Qualification?.Trim(),
            Email = request.Email?.Trim(),
            Phone = request.Phone?.Trim(),
            DepartmentId = department.Id,
            Department = department,
            RoomId = room?.Id,
            Room = room,
            Description = request.Description?.Trim(),
            AvatarUrl = avatarUrl,
            ExperienceYears = request.ExperienceYears!.Value,
            ConsultationFee = request.ConsultationFee!.Value,
            IsActive = true
        };

        _context.Doctors.Add(doctor);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch
        {
            // Nếu lưu DB lỗi, xóa ảnh vừa tạo (không để file rác)
            if (avatarUrl != null)
            {
                _imageHelper.DeleteImage(avatarUrl);
            }
            throw;
        }

        return MapToResponse(doctor);
    }

    public async Task<DoctorResponse> UpdateAsync(int id, UpdateDoctorRequest request)
    {
        var doctor = await _context.Doctors
            .Include(d => d.Department)
            .Include(d => d.Room)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (doctor == null || !doctor.IsActive)
        {
            throw new NotFoundException(ErrorCode.DOCTOR_NOT_FOUND);
        }

        // Validate DepartmentId tồn tại và còn hoạt động
        var department = await _context.Departments.FindAsync(request.DepartmentId!.Value);
        if (department == null || !department.IsActive)
        {
            throw new BadRequestException(ErrorCode.DEPARTMENT_NOT_FOUND);
        }

        // Validate RoomId (nếu có)
        Room? room = null;
        if (request.RoomId.HasValue)
        {
            room = await _context.Rooms.FindAsync(request.RoomId.Value);
            if (room == null || !room.IsActive)
            {
                throw new BadRequestException(ErrorCode.ROOM_NOT_FOUND);
            }
            if (room.DepartmentId != request.DepartmentId!.Value)
            {
                throw new BadRequestException(ErrorCode.ROOM_NOT_IN_DEPARTMENT);
            }
        }

        // Validate Email trùng (loại trừ chính bác sĩ đang sửa)
        await ValidateEmailUniqueAsync(request.Email, excludeDoctorId: id);

        // Xử lý ảnh
        string? newAvatarUrl = null;
        var oldAvatarUrl = doctor.AvatarUrl;

        if (request.Image != null)
        {
            // Có ảnh mới → lưu ảnh mới trước
            newAvatarUrl = await _imageHelper.SaveImageAsync(request.Image, "doctors");
        }

        // Cập nhật tất cả trường (PUT thay thế toàn bộ, trường optional không gửi → null)
        doctor.FullName = request.FullName.Trim();
        doctor.Qualification = request.Qualification?.Trim();
        doctor.Email = request.Email?.Trim();
        doctor.Phone = request.Phone?.Trim();
        doctor.DepartmentId = department.Id;
        doctor.Department = department;
        doctor.RoomId = room?.Id;
        doctor.Room = room;
        doctor.Description = request.Description?.Trim();
        doctor.ExperienceYears = request.ExperienceYears!.Value;
        doctor.ConsultationFee = request.ConsultationFee!.Value;

        // Ảnh: chỉ cập nhật AvatarUrl khi có ảnh mới
        if (newAvatarUrl != null)
        {
            doctor.AvatarUrl = newAvatarUrl;
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch
        {
            // Nếu lưu DB lỗi, xóa ảnh mới vừa tạo
            if (newAvatarUrl != null)
            {
                _imageHelper.DeleteImage(newAvatarUrl);
            }
            throw;
        }

        // Lưu DB thành công → xóa ảnh cũ (nếu có ảnh mới thay thế)
        if (newAvatarUrl != null && !string.IsNullOrWhiteSpace(oldAvatarUrl))
        {
            _imageHelper.DeleteImage(oldAvatarUrl);
        }

        return MapToResponse(doctor);
    }

    public async Task DeleteAsync(int id)
    {
        var doctor = await _context.Doctors.FindAsync(id);

        if (doctor == null || !doctor.IsActive)
        {
            throw new NotFoundException(ErrorCode.DOCTOR_NOT_FOUND);
        }

        // Chặn xóa khi bác sĩ đã có trong đơn hàng (OrderDetails)
        var hasOrders = await _context.OrderDetails.AnyAsync(od => od.DoctorId == id);
        if (hasOrders)
        {
            throw new BadRequestException(ErrorCode.DOCTOR_HAS_ORDERS);
        }

        // Xóa mềm: IsActive = false, KHÔNG xóa file ảnh
        doctor.IsActive = false;
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Kiểm tra Email trùng giữa các bác sĩ đang hoạt động.
    /// Bỏ qua khi Email null hoặc rỗng.
    /// So sánh không phân biệt hoa thường sau Trim.
    /// </summary>
    private async Task ValidateEmailUniqueAsync(string? email, int? excludeDoctorId)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return;
        }

        var trimmedEmail = email.Trim().ToLowerInvariant();

        var exists = await _context.Doctors
            .AnyAsync(d => d.IsActive
                && d.Email != null
                && d.Email.ToLower() == trimmedEmail
                && (!excludeDoctorId.HasValue || d.Id != excludeDoctorId.Value));

        if (exists)
        {
            throw new BadRequestException(ErrorCode.DOCTOR_EMAIL_ALREADY_EXISTS);
        }
    }

    private static DoctorResponse MapToResponse(Entities.Doctor d)
    {
        return new DoctorResponse
        {
            Id = d.Id,
            FullName = d.FullName,
            Qualification = d.Qualification,
            Email = d.Email,
            Phone = d.Phone,
            DepartmentId = d.DepartmentId,
            DepartmentName = d.Department?.Name ?? string.Empty,
            RoomId = d.RoomId,
            RoomNumber = d.Room?.RoomNumber,
            Floor = d.Room != null ? d.Room.Floor : null,
            Description = d.Description,
            AvatarUrl = d.AvatarUrl,
            ExperienceYears = d.ExperienceYears,
            ConsultationFee = d.ConsultationFee,
            IsActive = d.IsActive,
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt
        };
    }
}
