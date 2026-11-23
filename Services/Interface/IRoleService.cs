using MedicalAppointment.API.DTOs.Role;

namespace MedicalAppointment.API.Services.Interface;

public interface IRoleService
{
    Task<List<RoleResponse>> GetAllAsync();
    Task<RoleResponse> GetByIdAsync(int id);
    Task<RoleResponse> CreateAsync(CreateRoleRequest request);
    Task<RoleResponse> UpdateAsync(int id, UpdateRoleRequest request);
    Task DeleteAsync(int id);
}
