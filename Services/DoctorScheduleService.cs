using MedicalAppointment.API.Data;
using MedicalAppointment.API.DTOs.DoctorSchedule;
using MedicalAppointment.API.Entities;
using MedicalAppointment.API.Exceptions;
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
        var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == doctorId && d.IsActive);
        if (!doctorExists)
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
        var schedule = await _context.DoctorSchedules.FindAsync(id);
        if (schedule == null)
        {
            throw new NotFoundException(ErrorCode.RESOURCE_NOT_FOUND);
        }

        var hasConflict = await _context.DoctorSchedules
            .AnyAsync(ds => ds.Id != id 
                         && ds.DoctorId == schedule.DoctorId 
                         && ds.DayOfWeek == request.DayOfWeek!.Value 
                         && request.StartTime!.Value < ds.EndTime 
                         && request.EndTime!.Value > ds.StartTime);

        if (hasConflict)
        {
            throw new BadRequestException(ErrorCode.DOCTOR_SCHEDULE_CONFLICT);
        }

        schedule.DayOfWeek = request.DayOfWeek!.Value;
        schedule.StartTime = request.StartTime!.Value;
        schedule.EndTime = request.EndTime!.Value;
        
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

        _context.DoctorSchedules.Remove(schedule);
        await _context.SaveChangesAsync();
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
