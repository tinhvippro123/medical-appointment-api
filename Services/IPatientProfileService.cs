using MedicalAppointment.API.DTOs.PatientProfile;

namespace MedicalAppointment.API.Services;

public interface IPatientProfileService
{
    Task<IEnumerable<PatientProfileResponse>> GetAllByUserIdAsync(int userId);
    Task<PatientProfileResponse> CreateAsync(int userId, CreatePatientProfileRequest request);
    Task<PatientProfileResponse> UpdateAsync(int id, int userId, UpdatePatientProfileRequest request);
    Task DeleteAsync(int id, int userId);
}
