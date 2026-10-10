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
        ValidateScheduleInput(request.DayOfWeek, request.StartTime, request.EndTime, request.MaxPatients);

        var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == doctorId && d.IsActive);
        if (!doctorExists)
        {
            throw new NotFoundException(ErrorCode.DOCTOR_NOT_FOUND);
        }

        var hasConflict = await _context.DoctorSchedules
            .AnyAsync(ds => ds.DoctorId == doctorId 
                         && ds.DayOfWeek == request.DayOfWeek 
                         && request.StartTime < ds.EndTime 
                         && request.EndTime > ds.StartTime);
                         
        if (hasConflict)
        {
            throw new BadRequestException(ErrorCode.DOCTOR_SCHEDULE_CONFLICT);
        }

        var schedule = new DoctorSchedule
        {
            DoctorId = doctorId,
            DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            MaxPatients = request.MaxPatients ?? DefaultMaxPatients
        };

        _context.DoctorSchedules.Add(schedule);
        await _context.SaveChangesAsync();

        return MapToResponse(schedule);
    }

    public async Task<DoctorScheduleResponse> UpdateScheduleAsync(int id, UpdateDoctorScheduleRequest request)
    {
        ValidateScheduleInput(request.DayOfWeek, request.StartTime, request.EndTime, request.MaxPatients);

        var schedule = await _context.DoctorSchedules.FindAsync(id);
        if (schedule == null)
        {
            throw new NotFoundException(ErrorCode.RESOURCE_NOT_FOUND);
        }

        var hasConflict = await _context.DoctorSchedules
            .AnyAsync(ds => ds.Id != id 
                         && ds.DoctorId == schedule.DoctorId 
                         && ds.DayOfWeek == request.DayOfWeek 
                         && request.StartTime < ds.EndTime 
                         && request.EndTime > ds.StartTime);

        if (hasConflict)
        {
            throw new BadRequestException(ErrorCode.DOCTOR_SCHEDULE_CONFLICT);
        }

        schedule.DayOfWeek = request.DayOfWeek;
        schedule.StartTime = request.StartTime;
        schedule.EndTime = request.EndTime;
        
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

    private void ValidateScheduleInput(int dayOfWeek, TimeSpan startTime, TimeSpan endTime, int? maxPatients)
    {
        if (dayOfWeek < 0 || dayOfWeek > 6)
        {
            throw new BadRequestException(new ErrorCode("INVALID_DAY_OF_WEEK", "DayOfWeek phải từ 0 đến 6 (Chủ nhật đến Thứ 7)."));
        }

        if (startTime < TimeSpan.Zero || startTime >= TimeSpan.FromHours(24))
        {
            throw new BadRequestException(new ErrorCode("INVALID_TIME", "StartTime phải lớn hơn hoặc bằng 00:00:00 và nhỏ hơn 24:00:00."));
        }

        if (endTime <= TimeSpan.Zero || endTime >= TimeSpan.FromHours(24))
        {
            throw new BadRequestException(new ErrorCode("INVALID_TIME", "EndTime phải lớn hơn 00:00:00 và nhỏ hơn 24:00:00."));
        }

        if (startTime >= endTime)
        {
            throw new BadRequestException(new ErrorCode("INVALID_TIME_RANGE", "StartTime phải nhỏ hơn EndTime."));
        }

        if (maxPatients.HasValue && maxPatients.Value <= 0)
        {
            throw new BadRequestException(new ErrorCode("INVALID_MAX_PATIENTS", "MaxPatients phải lớn hơn 0."));
        }
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
