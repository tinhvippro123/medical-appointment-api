using MedicalAppointment.API.Data;
using MedicalAppointment.API.DTOs.Role;
using MedicalAppointment.API.Entities;
using MedicalAppointment.API.Exceptions;
using MedicalAppointment.API.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace MedicalAppointment.API.Services;

public class RoleService(ApplicationDbContext context) : IRoleService
{
    private static readonly string[] SystemRoleKeys = ["ADMIN", "PATIENT"];

    public async Task<List<RoleResponse>> GetAllAsync()
    {
        return await context.Roles
            .AsNoTracking()
            .OrderBy(r => r.Id)
            .Select(r => new RoleResponse
            {
                Id = r.Id,
                Key = r.Key,
                Name = r.Name,
                Description = r.Description,
                UserCount = r.Users.Count,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<RoleResponse> GetByIdAsync(int id)
    {
        var role = await context.Roles
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new RoleResponse
            {
                Id = r.Id,
                Key = r.Key,
                Name = r.Name,
                Description = r.Description,
                UserCount = r.Users.Count,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (role == null)
        {
            throw new NotFoundException(ErrorCode.ROLE_NOT_FOUND);
        }

        return role;
    }

    public async Task<RoleResponse> CreateAsync(CreateRoleRequest request)
    {
        var normalizedKey = NormalizeKey(request.Key);

        var keyExists = await context.Roles.AnyAsync(r => r.Key == normalizedKey);
        if (keyExists)
        {
            throw new BadRequestException(ErrorCode.ROLE_KEY_ALREADY_EXISTS);
        }

        var role = new Role
        {
            Key = normalizedKey,
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim()
        };

        context.Roles.Add(role);
        await context.SaveChangesAsync();

        return new RoleResponse
        {
            Id = role.Id,
            Key = role.Key,
            Name = role.Name,
            Description = role.Description,
            UserCount = 0,
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt
        };
    }

    public async Task<RoleResponse> UpdateAsync(int id, UpdateRoleRequest request)
    {
        var role = await context.Roles.FirstOrDefaultAsync(r => r.Id == id);
        if (role == null)
        {
            throw new NotFoundException(ErrorCode.ROLE_NOT_FOUND);
        }

        var normalizedKey = NormalizeKey(request.Key);
        var isSystemRole = SystemRoleKeys.Contains(role.Key);
        var isChangingKey = role.Key != normalizedKey;

        if (isSystemRole && isChangingKey)
        {
            throw new BadRequestException(ErrorCode.SYSTEM_ROLE_PROTECTED);
        }

        var keyExists = await context.Roles.AnyAsync(r => r.Key == normalizedKey && r.Id != id);
        if (keyExists)
        {
            throw new BadRequestException(ErrorCode.ROLE_KEY_ALREADY_EXISTS);
        }

        role.Key = normalizedKey;
        role.Name = request.Name.Trim();
        role.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        await context.SaveChangesAsync();

        var userCount = await context.Users.CountAsync(u => u.RoleId == id);
        return new RoleResponse
        {
            Id = role.Id,
            Key = role.Key,
            Name = role.Name,
            Description = role.Description,
            UserCount = userCount,
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt
        };
    }

    public async Task DeleteAsync(int id)
    {
        var role = await context.Roles.FirstOrDefaultAsync(r => r.Id == id);
        if (role == null)
        {
            throw new NotFoundException(ErrorCode.ROLE_NOT_FOUND);
        }

        if (SystemRoleKeys.Contains(role.Key))
        {
            throw new BadRequestException(ErrorCode.SYSTEM_ROLE_PROTECTED);
        }

        var isInUse = await context.Users.AnyAsync(u => u.RoleId == id);
        if (isInUse)
        {
            throw new BadRequestException(ErrorCode.ROLE_IN_USE);
        }

        context.Roles.Remove(role);
        await context.SaveChangesAsync();
    }

    private static string NormalizeKey(string key) => key.Trim().ToUpperInvariant();
}
