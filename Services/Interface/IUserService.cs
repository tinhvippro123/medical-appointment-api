using MedicalAppointment.API.DTOs.Common;
using MedicalAppointment.API.DTOs.User;

namespace MedicalAppointment.API.Services.Interface;

public interface IUserService
{
    Task<PagedResult<UserResponse>> GetAllAsync(UserQueryParameters query);
    Task<UserResponse> GetByIdAsync(int id);
    Task<UserResponse> CreateAsync(CreateUserRequest request);
    Task<UserResponse> UpdateAsync(int id, int currentUserId, UpdateUserRequest request);
    Task<UserResponse> UpdateRoleAsync(int id, int currentUserId, UpdateUserRoleRequest request);
    Task<UserResponse> UpdateStatusAsync(int id, int currentUserId, UpdateUserStatusRequest request);
    Task DeleteAsync(int id, int currentUserId);
}
