using MedicalAppointment.API.DTOs.Department;

namespace MedicalAppointment.API.Services;

public interface IDepartmentService
{
    Task<List<DepartmentDto>> GetAllAsync();
    Task<DepartmentDto> GetByIdAsync(int id);
    Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto);
    Task<DepartmentDto> UpdateAsync(int id, UpdateDepartmentDto dto);
    Task DeleteAsync(int id);
    Task<DepartmentDto> UploadImageAsync(int id, IFormFile file);
    Task<DepartmentDto> DeleteImageAsync(int id);
}

