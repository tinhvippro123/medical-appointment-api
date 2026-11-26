using MedicalAppointment.API.Data;
using MedicalAppointment.API.DTOs.Room;
using MedicalAppointment.API.Entities;
using MedicalAppointment.API.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace MedicalAppointment.API.Services;

public class RoomService(ApplicationDbContext context) : IRoomService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<List<RoomResponse>> GetAllAsync(int? departmentId)
    {
        var query = _context.Rooms.AsNoTracking().Where(r => r.IsActive);
        if (departmentId.HasValue)
            query = query.Where(r => r.DepartmentId == departmentId.Value);

        var rooms = await query.OrderBy(r => r.Id).ToListAsync();
        var result = new List<RoomResponse>();
        foreach (var room in rooms)
            result.Add(ToResponse(room));

        return result;
    }

    public async Task<RoomResponse> GetByIdAsync(int id)
    {
        var room = await FindRoomAsync(id);
        return ToResponse(room);
    }

    public async Task<RoomResponse> CreateAsync(RoomRequest request)
    {
        await CheckDepartmentAsync(request.DepartmentId);
        var room = new Room
        {
            RoomNumber = request.RoomNumber.Trim(),
            Floor = request.Floor,
            DepartmentId = request.DepartmentId
        };
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
        return ToResponse(room);
    }

    public async Task<RoomResponse> UpdateAsync(int id, RoomRequest request)
    {
        var room = await FindRoomAsync(id);
        await CheckDepartmentAsync(request.DepartmentId);

        room.RoomNumber = request.RoomNumber.Trim();
        room.Floor = request.Floor;
        room.DepartmentId = request.DepartmentId;
        await _context.SaveChangesAsync();
        return ToResponse(room);
    }

    public async Task DeleteAsync(int id)
    {
        var room = await FindRoomAsync(id);
        room.IsActive = false;

        // Gỡ phòng khỏi các bác sĩ đang trỏ tới phòng bị xóa mềm
        var assignedDoctors = await _context.Doctors
            .Where(d => d.RoomId == id)
            .ToListAsync();
        foreach (var doctor in assignedDoctors)
        {
            doctor.RoomId = null;
        }

        await _context.SaveChangesAsync();
    }

    private async Task<Room> FindRoomAsync(int id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room == null || !room.IsActive)
            throw new NotFoundException(ErrorCode.ROOM_NOT_FOUND);
        return room;
    }

    private async Task CheckDepartmentAsync(int departmentId)
    {
        var department = await _context.Departments.FindAsync(departmentId);
        if (department == null || !department.IsActive)
            throw new NotFoundException(ErrorCode.DEPARTMENT_NOT_FOUND);
    }

    // Không trả navigation properties để tránh vòng lặp khi chuyển sang JSON.
    private static RoomResponse ToResponse(Room room)
    {
        return new RoomResponse
        {
            Id = room.Id,
            RoomNumber = room.RoomNumber,
            Floor = room.Floor,
            DepartmentId = room.DepartmentId,
            IsActive = room.IsActive,
            CreatedAt = room.CreatedAt,
            UpdatedAt = room.UpdatedAt
        };
    }
}
