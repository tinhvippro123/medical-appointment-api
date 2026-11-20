using MedicalAppointment.API.DTOs.Room;

namespace MedicalAppointment.API.Services;

public interface IRoomService
{
    Task<List<RoomResponse>> GetAllAsync(int? departmentId);
    Task<RoomResponse> GetByIdAsync(int id);
    Task<RoomResponse> CreateAsync(RoomRequest request);
    Task<RoomResponse> UpdateAsync(int id, RoomRequest request);
    Task DeleteAsync(int id);
}
