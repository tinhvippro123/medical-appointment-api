using MedicalAppointment.API.DTOs.Common;
using MedicalAppointment.API.DTOs.Department;
using MedicalAppointment.API.DTOs.Doctor;

namespace MedicalAppointment.API.Services.Interface;

public interface IDepartmentService
{
    Task<List<DepartmentDto>> GetAllAsync();
    Task<DepartmentDto> GetByIdAsync(int id);
    Task<PagedResult<DoctorResponse>> GetDoctorsAsync(int departmentId, int page, int pageSize);
    Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto);
    Task<DepartmentDto> UpdateAsync(int id, UpdateDepartmentDto dto);
    Task DeleteAsync(int id);
    Task<DepartmentDto> UploadImageAsync(int id, IFormFile file);
    Task<DepartmentDto> SetImageUrlAsync(int id, string imageUrl);
    Task<DepartmentDto> DeleteImageAsync(int id);
}

