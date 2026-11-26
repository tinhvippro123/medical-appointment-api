using MedicalAppointment.API.Data;
using MedicalAppointment.API.DTOs.DoctorSchedule;
using MedicalAppointment.API.Entities;
using MedicalAppointment.API.Exceptions;
using MedicalAppointment.API.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace MedicalAppointment.API.Services;

public class DoctorScheduleService(ApplicationDbContext context) : IDoctorScheduleService
{
    private const int DefaultMaxPatients = 20;
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<DoctorScheduleResponse>> GetSchedulesByDoctorIdAsync(int doctorId)
    {
        var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == doctorId && d.IsActive);
        if (!doctorExists)
        {
            throw new NotFoundException(ErrorCode.DOCTOR_NOT_FOUND);
        }

        var schedules = await _context.DoctorSchedules
            .Where(ds => ds.DoctorId == doctorId)
            .OrderBy(ds => ds.DayOfWeek)
            .ThenBy(ds => ds.StartTime)
            .AsNoTracking()
            .ToListAsync();

        return schedules.Select(MapToResponse);
    }

    public async Task<DoctorScheduleResponse> CreateScheduleAsync(int doctorId, CreateDoctorScheduleRequest request)
    {
        var doctor = await _context.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == doctorId && d.IsActive && d.Department.IsActive);
        if (doctor == null)
        {
            throw new NotFoundException(ErrorCode.DOCTOR_NOT_FOUND);
        }

        var hasConflict = await _context.DoctorSchedules
            .AnyAsync(ds => ds.DoctorId == doctorId 
                         && ds.DayOfWeek == request.DayOfWeek!.Value 
                         && request.StartTime!.Value < ds.EndTime 
                         && request.EndTime!.Value > ds.StartTime);
                         
        if (hasConflict)
        {
            throw new BadRequestException(ErrorCode.DOCTOR_SCHEDULE_CONFLICT);
        }

        if (doctor.RoomId.HasValue)
        {
            var hasRoomConflict = await _context.DoctorSchedules
                .AnyAsync(ds => ds.DoctorId != doctorId
                             && ds.Doctor.IsActive
                             && ds.Doctor.RoomId == doctor.RoomId.Value
                             && ds.DayOfWeek == request.DayOfWeek!.Value
                             && request.StartTime!.Value < ds.EndTime
                             && request.EndTime!.Value > ds.StartTime);
            if (hasRoomConflict)
            {
                throw new BadRequestException(ErrorCode.ROOM_SCHEDULE_CONFLICT);
            }
        }

        var schedule = new DoctorSchedule
        {
            DoctorId = doctorId,
            DayOfWeek = request.DayOfWeek!.Value,
            StartTime = request.StartTime!.Value,
            EndTime = request.EndTime!.Value,
            MaxPatients = request.MaxPatients ?? DefaultMaxPatients
        };

        _context.DoctorSchedules.Add(schedule);
        await _context.SaveChangesAsync();

        return MapToResponse(schedule);
    }

    public async Task<DoctorScheduleResponse> UpdateScheduleAsync(int id, UpdateDoctorScheduleRequest request)
    {
        var schedule = await _context.DoctorSchedules
            .Include(ds => ds.Doctor)
            .FirstOrDefaultAsync(ds => ds.Id == id);
        if (schedule == null)
        {
            throw new NotFoundException(ErrorCode.RESOURCE_NOT_FOUND);
        }

        var newDayOfWeek = request.DayOfWeek!.Value;
        var newStart = request.StartTime!.Value;
        var newEnd = request.EndTime!.Value;

        var hasConflict = await _context.DoctorSchedules
            .AnyAsync(ds => ds.Id != id 
                         && ds.DoctorId == schedule.DoctorId 
                         && ds.DayOfWeek == newDayOfWeek 
                         && newStart < ds.EndTime 
                         && newEnd > ds.StartTime);

        if (hasConflict)
        {
            throw new BadRequestException(ErrorCode.DOCTOR_SCHEDULE_CONFLICT);
        }

        if (schedule.Doctor.RoomId.HasValue)
        {
            var roomId = schedule.Doctor.RoomId.Value;
            var hasRoomConflict = await _context.DoctorSchedules
                .AnyAsync(ds => ds.DoctorId != schedule.DoctorId
                             && ds.Doctor.IsActive
                             && ds.Doctor.RoomId == roomId
                             && ds.DayOfWeek == newDayOfWeek
                             && newStart < ds.EndTime
                             && newEnd > ds.StartTime);
            if (hasRoomConflict)
            {
                throw new BadRequestException(ErrorCode.ROOM_SCHEDULE_CONFLICT);
            }
        }

        // Chặn đổi thứ hoặc thu hẹp ca làm việc nếu đã có lịch hẹn tương lai nằm trong ca cũ mà không còn khớp ca mới
        if (await HasAffectedFutureOrdersAsync(
                schedule.DoctorId,
                schedule.DayOfWeek,
                schedule.StartTime,
                schedule.EndTime,
                newDayOfWeek,
                newStart,
                newEnd))
        {
            throw new BadRequestException(ErrorCode.DOCTOR_SCHEDULE_HAS_ORDERS);
        }

        schedule.DayOfWeek = newDayOfWeek;
        schedule.StartTime = newStart;
        schedule.EndTime = newEnd;
        
        if (request.MaxPatients.HasValue)
        {
            schedule.MaxPatients = request.MaxPatients.Value;
        }

        await _context.SaveChangesAsync();

        return MapToResponse(schedule);
    }

    public async Task DeleteScheduleAsync(int id)
    {
        var schedule = await _context.DoctorSchedules.FindAsync(id);
        if (schedule == null)
        {
            throw new NotFoundException(ErrorCode.RESOURCE_NOT_FOUND);
        }

        if (await HasAffectedFutureOrdersAsync(
                schedule.DoctorId,
                schedule.DayOfWeek,
                schedule.StartTime,
                schedule.EndTime,
                newDayOfWeek: null,
                newStart: null,
                newEnd: null))
        {
            throw new BadRequestException(ErrorCode.DOCTOR_SCHEDULE_HAS_ORDERS);
        }

        _context.DoctorSchedules.Remove(schedule);
        await _context.SaveChangesAsync();
    }

    private async Task<bool> HasAffectedFutureOrdersAsync(
        int doctorId,
        int oldDayOfWeek,
        TimeSpan oldStart,
        TimeSpan oldEnd,
        int? newDayOfWeek,
        TimeSpan? newStart,
        TimeSpan? newEnd)
    {
        var todayVn = DateTime.UtcNow.AddHours(7).Date;
        var futureDetails = await _context.OrderDetails
            .AsNoTracking()
            .Where(od => od.DoctorId == doctorId &&
                         od.AppointmentDate >= todayVn &&
                         od.Status != "Cancelled")
            .Select(od => new { od.AppointmentDate, od.TimeSlot })
            .ToListAsync();

        foreach (var detail in futureDetails)
        {
            if ((int)detail.AppointmentDate.DayOfWeek != oldDayOfWeek)
            {
                continue;
            }

            var parts = detail.TimeSlot.Trim().Split('-');
            if (parts.Length != 2 ||
                !TimeSpan.TryParse(parts[0].Trim(), out var slotStart) ||
                !TimeSpan.TryParse(parts[1].Trim(), out var slotEnd))
            {
                continue;
            }

            var wasInOldShift = oldStart <= slotStart && oldEnd >= slotEnd;
            if (!wasInOldShift)
            {
                continue;
            }

            if (newDayOfWeek == null || newStart == null || newEnd == null)
            {
                return true;
            }

            var stillFitsNewShift = newDayOfWeek.Value == oldDayOfWeek &&
                                    newStart.Value <= slotStart &&
                                    newEnd.Value >= slotEnd;
            if (!stillFitsNewShift)
            {
                return true;
            }
        }

        return false;
    }

    private DoctorScheduleResponse MapToResponse(DoctorSchedule schedule)
    {
        return new DoctorScheduleResponse
        {
            Id = schedule.Id,
            DoctorId = schedule.DoctorId,
            DayOfWeek = schedule.DayOfWeek,
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime,
            MaxPatients = schedule.MaxPatients
        };
    }
}
