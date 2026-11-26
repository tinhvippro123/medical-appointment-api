using System.Data;
using MedicalAppointment.API.Data;
using MedicalAppointment.API.DTOs.DoctorDayOff;
using MedicalAppointment.API.Entities;
using MedicalAppointment.API.Exceptions;
using MedicalAppointment.API.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace MedicalAppointment.API.Services;

public class DoctorDayOffService(ApplicationDbContext context) : IDoctorDayOffService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<DoctorDayOffResponse>> GetDayOffsByDoctorIdAsync(int doctorId, DoctorDayOffQueryParameters query)
    {
        var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == doctorId && d.IsActive && d.Department.IsActive);
        if (!doctorExists)
        {
            throw new NotFoundException(ErrorCode.DOCTOR_NOT_FOUND);
        }

        var baseQuery = _context.DoctorDayOffs
            .AsNoTracking()
            .Where(ddo => ddo.DoctorId == doctorId);

        if (query.From.HasValue)
        {
            var fromDate = query.From.Value.ToDateTime(TimeOnly.MinValue);
            baseQuery = baseQuery.Where(ddo => ddo.Date >= fromDate);
        }

        if (query.To.HasValue)
        {
            var toDate = query.To.Value.ToDateTime(TimeOnly.MinValue);
            baseQuery = baseQuery.Where(ddo => ddo.Date <= toDate);
        }

        var dayOffs = await baseQuery
            .OrderBy(ddo => ddo.Date)
            .ThenBy(ddo => ddo.Id)
            .ToListAsync();

        return dayOffs.Select(MapToResponse);
    }

    public async Task<DoctorDayOffResponse> CreateDayOffAsync(int doctorId, CreateDoctorDayOffRequest request)
    {
        var todayVn = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
        var requestDate = request.Date!.Value;
        if (requestDate < todayVn)
        {
            throw new BadRequestException(ErrorCode.DAY_OFF_IN_PAST);
        }

        var targetDate = requestDate.ToDateTime(TimeOnly.MinValue);
        var startOfDay = targetDate.Date;
        var endOfDay = startOfDay.AddDays(1);

        // Dùng Serializable transaction để ngăn write skew với OrderService.CheckoutAsync
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == doctorId && d.IsActive);
        if (!doctorExists)
        {
            throw new NotFoundException(ErrorCode.DOCTOR_NOT_FOUND);
        }

        var isDuplicate = await _context.DoctorDayOffs
            .AnyAsync(ddo => ddo.DoctorId == doctorId &&
                             ddo.Date >= startOfDay &&
                             ddo.Date < endOfDay);
        if (isDuplicate)
        {
            throw new BadRequestException(ErrorCode.DAY_OFF_ALREADY_EXISTS);
        }

        // Kiểm tra số đơn bị ảnh hưởng (dùng đúng định nghĩa như phần sức chứa trong OrderService)
        var affectedOrdersCount = await _context.OrderDetails
            .Where(od => od.DoctorId == doctorId &&
                         od.AppointmentDate >= startOfDay &&
                         od.AppointmentDate < endOfDay &&
                         od.Status != "Cancelled")
            .Select(od => od.OrderId)
            .Distinct()
            .CountAsync();

        if (affectedOrdersCount > 0)
        {
            throw new BadRequestException(ErrorCode.DoctorDayOffHasOrders(affectedOrdersCount));
        }

        var dayOff = new DoctorDayOff
        {
            DoctorId = doctorId,
            Date = targetDate,
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim()
        };

        _context.DoctorDayOffs.Add(dayOff);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return MapToResponse(dayOff);
    }

    public async Task DeleteDayOffAsync(int doctorId, int id)
    {
        var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == doctorId && d.IsActive);
        if (!doctorExists)
        {
            throw new NotFoundException(ErrorCode.DOCTOR_NOT_FOUND);
        }

        var dayOff = await _context.DoctorDayOffs
            .FirstOrDefaultAsync(ddo => ddo.Id == id && ddo.DoctorId == doctorId);

        if (dayOff == null)
        {
            throw new NotFoundException(ErrorCode.DAY_OFF_NOT_FOUND);
        }

        _context.DoctorDayOffs.Remove(dayOff);
        await _context.SaveChangesAsync();
    }

    private static DoctorDayOffResponse MapToResponse(DoctorDayOff dayOff) =>
        new()
        {
            Id = dayOff.Id,
            DoctorId = dayOff.DoctorId,
            Date = DateOnly.FromDateTime(dayOff.Date),
            Reason = dayOff.Reason,
            CreatedAt = dayOff.CreatedAt,
            UpdatedAt = dayOff.UpdatedAt
        };
}
