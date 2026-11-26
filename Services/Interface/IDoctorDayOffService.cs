using MedicalAppointment.API.DTOs.DoctorDayOff;

namespace MedicalAppointment.API.Services.Interface;

public interface IDoctorDayOffService
{
    Task<IEnumerable<DoctorDayOffResponse>> GetDayOffsByDoctorIdAsync(int doctorId, DoctorDayOffQueryParameters query);
    Task<DoctorDayOffResponse> CreateDayOffAsync(int doctorId, CreateDoctorDayOffRequest request);
    Task DeleteDayOffAsync(int doctorId, int id);
}
