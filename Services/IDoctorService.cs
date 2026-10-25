using MedicalAppointment.API.DTOs.Common;
using MedicalAppointment.API.DTOs.Doctor;

namespace MedicalAppointment.API.Services;

public interface IDoctorService
{
    Task<PagedResult<DoctorResponse>> GetAllAsync(DoctorQueryParameters query);
    Task<DoctorDetailResponse> GetByIdAsync(int id);
    Task<PagedResult<DoctorResponse>> GetRelatedAsync(int id, RelatedDoctorQueryParameters query);
    Task<DoctorResponse> CreateAsync(CreateDoctorRequest request);
    Task<DoctorResponse> UpdateAsync(int id, UpdateDoctorRequest request);
    Task DeleteAsync(int id);
}
