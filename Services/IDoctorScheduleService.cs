using MedicalAppointment.API.DTOs.DoctorSchedule;

namespace MedicalAppointment.API.Services;

public interface IDoctorScheduleService
{
    Task<IEnumerable<DoctorScheduleResponse>> GetSchedulesByDoctorIdAsync(int doctorId);
    Task<DoctorScheduleResponse> CreateScheduleAsync(int doctorId, CreateDoctorScheduleRequest request);
    Task<DoctorScheduleResponse> UpdateScheduleAsync(int id, UpdateDoctorScheduleRequest request);
    Task DeleteScheduleAsync(int id);
}
