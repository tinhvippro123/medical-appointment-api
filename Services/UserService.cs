using MedicalAppointment.API.Data;
using MedicalAppointment.API.DTOs.Common;
using MedicalAppointment.API.DTOs.User;
using MedicalAppointment.API.Entities;
using MedicalAppointment.API.Exceptions;
using MedicalAppointment.API.Provider;
using MedicalAppointment.API.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace MedicalAppointment.API.Services;

public class UserService(ApplicationDbContext context, IPasswordHasher passwordHasher) : IUserService
{
    public async Task<PagedResult<UserResponse>> GetAllAsync(UserQueryParameters query)
    {
        var baseQuery = context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var term = query.Keyword.Trim();
            baseQuery = baseQuery.Where(u =>
                u.FullName.Contains(term) ||
                u.Phone.Contains(term) ||
                (u.Email != null && u.Email.Contains(term)));
        }

        if (query.RoleId.HasValue)
        {
            baseQuery = baseQuery.Where(u => u.RoleId == query.RoleId.Value);
        }

        if (query.IsActive.HasValue)
        {
            baseQuery = baseQuery.Where(u => u.IsActive == query.IsActive.Value);
        }

        var totalCount = await baseQuery.CountAsync();

        var users = await baseQuery
            .OrderByDescending(u => u.CreatedAt)
            .ThenByDescending(u => u.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResult<UserResponse>
        {
            Items = users.Select(MapToResponse).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<UserResponse> GetByIdAsync(int id)
    {
        var user = await context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            throw new NotFoundException(ErrorCode.USER_NOT_FOUND);
        }

        return MapToResponse(user);
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request)
    {
        var phone = request.Phone.Trim();
        var email = NormalizeEmail(request.Email);

        await ValidatePhoneAndEmailUniqueAsync(phone, email);
        var role = await GetRoleOrThrowAsync(request.RoleId);

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Phone = phone,
            Email = email,
            Password = passwordHasher.Hash(request.Password),
            RoleId = role.Id,
            Role = role,
            IsActive = true
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return MapToResponse(user);
    }

    public async Task<UserResponse> UpdateAsync(int id, int currentUserId, UpdateUserRequest request)
    {
        var user = await context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            throw new NotFoundException(ErrorCode.USER_NOT_FOUND);
        }

        var isSelf = id == currentUserId;
        if (isSelf && !request.IsActive)
        {
            throw new BadRequestException(ErrorCode.CANNOT_DISABLE_SELF);
        }

        if (isSelf && user.RoleId != request.RoleId)
        {
            throw new BadRequestException(ErrorCode.CANNOT_CHANGE_OWN_ROLE);
        }

        var phone = request.Phone.Trim();
        var email = NormalizeEmail(request.Email);

        await ValidatePhoneAndEmailUniqueAsync(phone, email, excludeUserId: id);
        var role = await GetRoleOrThrowAsync(request.RoleId);

        user.FullName = request.FullName.Trim();
        user.Phone = phone;
        user.Email = email;
        user.RoleId = role.Id;
        user.Role = role;
        user.IsActive = request.IsActive;

        await context.SaveChangesAsync();

        return MapToResponse(user);
    }

    public async Task<UserResponse> UpdateRoleAsync(int id, int currentUserId, UpdateUserRoleRequest request)
    {
        var user = await context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            throw new NotFoundException(ErrorCode.USER_NOT_FOUND);
        }

        if (id == currentUserId && user.RoleId != request.RoleId)
        {
            throw new BadRequestException(ErrorCode.CANNOT_CHANGE_OWN_ROLE);
        }

        var role = await GetRoleOrThrowAsync(request.RoleId);

        user.RoleId = role.Id;
        user.Role = role;

        await context.SaveChangesAsync();

        return MapToResponse(user);
    }

    public async Task<UserResponse> UpdateStatusAsync(int id, int currentUserId, UpdateUserStatusRequest request)
    {
        var user = await context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            throw new NotFoundException(ErrorCode.USER_NOT_FOUND);
        }

        if (id == currentUserId && !request.IsActive)
        {
            throw new BadRequestException(ErrorCode.CANNOT_DISABLE_SELF);
        }

        user.IsActive = request.IsActive;
        await context.SaveChangesAsync();

        return MapToResponse(user);
    }

    public async Task DeleteAsync(int id, int currentUserId)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            throw new NotFoundException(ErrorCode.USER_NOT_FOUND);
        }

        if (id == currentUserId)
        {
            throw new BadRequestException(ErrorCode.CANNOT_DISABLE_SELF);
        }

        // Xóa mềm: khóa tài khoản để giữ toàn vẹn lịch sử đơn hàng & hồ sơ khám
        user.IsActive = false;
        await context.SaveChangesAsync();
    }

    private static string? NormalizeEmail(string? email) =>
        string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();

    private async Task ValidatePhoneAndEmailUniqueAsync(string phone, string? email, int? excludeUserId = null)
    {
        var phoneExists = await context.Users.AnyAsync(u =>
            u.Phone == phone && (!excludeUserId.HasValue || u.Id != excludeUserId.Value));

        if (phoneExists)
        {
            throw new BadRequestException(ErrorCode.PHONE_ALREADY_EXISTS);
        }

        if (email != null)
        {
            var emailExists = await context.Users.AnyAsync(u =>
                u.Email != null &&
                u.Email.ToLower() == email &&
                (!excludeUserId.HasValue || u.Id != excludeUserId.Value));

            if (emailExists)
            {
                throw new BadRequestException(ErrorCode.EMAIL_ALREADY_EXISTS);
            }
        }
    }

    private async Task<Role> GetRoleOrThrowAsync(int roleId)
    {
        var role = await context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
        if (role == null)
        {
            throw new BadRequestException(ErrorCode.ROLE_NOT_FOUND);
        }

        return role;
    }

    private static UserResponse MapToResponse(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        Phone = user.Phone,
        AvatarUrl = user.AvatarUrl,
        RoleId = user.RoleId,
        RoleKey = user.Role?.Key ?? string.Empty,
        RoleName = user.Role?.Name ?? string.Empty,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt
    };
}
