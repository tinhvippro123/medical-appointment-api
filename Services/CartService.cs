using MedicalAppointment.API.Data;
using MedicalAppointment.API.DTOs.Cart;
using MedicalAppointment.API.Entities;
using MedicalAppointment.API.Exceptions;
using MedicalAppointment.API.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace MedicalAppointment.API.Services;

public class CartService(ApplicationDbContext dbContext) : ICartService
{
    public async Task<CartItemResponse> AddToCartAsync(int userId, AddCartItemRequest request)
    {
        var doctorExists = await dbContext.Doctors
            .AsNoTracking()
            .AnyAsync(d => d.Id == request.DoctorId!.Value && d.IsActive && d.Department.IsActive);
        if (!doctorExists)
        {
            throw new NotFoundException(ErrorCode.DOCTOR_NOT_FOUND);
        }

        var profileExists = await dbContext.PatientProfiles
            .AsNoTracking()
            .AnyAsync(p => p.Id == request.PatientProfileId!.Value && p.UserId == userId);
        if (!profileExists)
        {
            throw new BadRequestException(ErrorCode.INVALID_PATIENT_PROFILE);
        }

        var appointmentDate = request.AppointmentDate!.Value;
        var cleanSlot = request.TimeSlot.Trim();
        var (slotStart, slotEnd) = ParseAndValidateSlot(appointmentDate, cleanSlot);

        var dayOfWeek = (int)appointmentDate.DayOfWeek;
        var hasValidSchedule = await dbContext.DoctorSchedules
            .AsNoTracking()
            .AnyAsync(s => s.DoctorId == request.DoctorId!.Value &&
                           s.DayOfWeek == dayOfWeek &&
                           s.StartTime <= slotStart &&
                           s.EndTime >= slotEnd);
        if (!hasValidSchedule)
        {
            throw new BadRequestException(ErrorCode.INVALID_TIME_SLOT);
        }

        var apptDateTime = appointmentDate.ToDateTime(TimeOnly.MinValue);
        var startOfDay = apptDateTime.Date;
        var endOfDay = startOfDay.AddDays(1);

        var isOnLeave = await dbContext.DoctorDayOffs
            .AsNoTracking()
            .AnyAsync(ddo => ddo.DoctorId == request.DoctorId!.Value &&
                             ddo.Date >= startOfDay &&
                             ddo.Date < endOfDay);
        if (isOnLeave)
        {
            throw new BadRequestException(ErrorCode.DOCTOR_ON_LEAVE);
        }

        var existingCartItems = await dbContext.CartItems
            .AsNoTracking()
            .Where(ci => ci.UserId == userId &&
                         ci.PatientProfileId == request.PatientProfileId!.Value &&
                         ci.AppointmentDate == apptDateTime)
            .Select(ci => new { ci.DoctorId, ci.TimeSlot })
            .ToListAsync();

        foreach (var existing in existingCartItems)
        {
            if (existing.DoctorId == request.DoctorId!.Value && existing.TimeSlot == cleanSlot)
            {
                throw new BadRequestException(ErrorCode.CART_ITEM_DUPLICATE);
            }

            if (TryParseSlot(existing.TimeSlot, out var exStart, out var exEnd) &&
                slotStart < exEnd && slotEnd > exStart)
            {
                throw new BadRequestException(ErrorCode.PATIENT_SCHEDULE_CONFLICT);
            }
        }

        var cartItem = new CartItem
        {
            UserId = userId,
            DoctorId = request.DoctorId!.Value,
            PatientProfileId = request.PatientProfileId!.Value,
            AppointmentDate = apptDateTime,
            TimeSlot = cleanSlot,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim()
        };

        dbContext.CartItems.Add(cartItem);
        await dbContext.SaveChangesAsync();

        return await dbContext.CartItems
            .AsNoTracking()
            .Where(ci => ci.Id == cartItem.Id)
            .Select(ci => new CartItemResponse
            {
                Id = ci.Id,
                DoctorId = ci.DoctorId,
                DoctorFullName = ci.Doctor.FullName,
                DoctorAvatarUrl = ci.Doctor.AvatarUrl,
                DoctorIsActive = ci.Doctor.IsActive,
                DepartmentName = ci.Doctor.Department.Name,
                ConsultationFee = ci.Doctor.Department.ConsultationFee,
                PatientProfileId = ci.PatientProfileId,
                PatientProfileFullName = ci.PatientProfile.FullName,
                AppointmentDate = DateOnly.FromDateTime(ci.AppointmentDate),
                TimeSlot = ci.TimeSlot,
                Note = ci.Note,
                CreatedAt = ci.CreatedAt
            })
            .FirstAsync();
    }

    public async Task<IEnumerable<CartItemResponse>> GetCartAsync(int userId)
    {
        return await dbContext.CartItems
            .AsNoTracking()
            .Where(ci => ci.UserId == userId)
            .OrderByDescending(ci => ci.CreatedAt)
            .ThenByDescending(ci => ci.Id)
            .Select(ci => new CartItemResponse
            {
                Id = ci.Id,
                DoctorId = ci.DoctorId,
                DoctorFullName = ci.Doctor.FullName,
                DoctorAvatarUrl = ci.Doctor.AvatarUrl,
                DoctorIsActive = ci.Doctor.IsActive,
                DepartmentName = ci.Doctor.Department.Name,
                ConsultationFee = ci.Doctor.Department.ConsultationFee,
                PatientProfileId = ci.PatientProfileId,
                PatientProfileFullName = ci.PatientProfile.FullName,
                AppointmentDate = DateOnly.FromDateTime(ci.AppointmentDate),
                TimeSlot = ci.TimeSlot,
                Note = ci.Note,
                CreatedAt = ci.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<CartItemResponse> UpdateCartItemAsync(int userId, int cartItemId, UpdateCartItemRequest request)
    {
        var cartItem = await dbContext.CartItems
            .Include(ci => ci.Doctor)
                .ThenInclude(d => d.Department)
            .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.UserId == userId);
        if (cartItem == null)
        {
            throw new NotFoundException(ErrorCode.RESOURCE_NOT_FOUND);
        }

        if (!cartItem.Doctor.IsActive || !cartItem.Doctor.Department.IsActive)
        {
            throw new NotFoundException(ErrorCode.DOCTOR_NOT_FOUND);
        }

        var appointmentDate = request.AppointmentDate!.Value;
        var cleanSlot = request.TimeSlot.Trim();
        var (slotStart, slotEnd) = ParseAndValidateSlot(appointmentDate, cleanSlot);

        var dayOfWeek = (int)appointmentDate.DayOfWeek;
        var hasValidSchedule = await dbContext.DoctorSchedules
            .AsNoTracking()
            .AnyAsync(s => s.DoctorId == cartItem.DoctorId &&
                           s.DayOfWeek == dayOfWeek &&
                           s.StartTime <= slotStart &&
                           s.EndTime >= slotEnd);
        if (!hasValidSchedule)
        {
            throw new BadRequestException(ErrorCode.INVALID_TIME_SLOT);
        }

        var apptDateTime = appointmentDate.ToDateTime(TimeOnly.MinValue);
        var startOfDay = apptDateTime.Date;
        var endOfDay = startOfDay.AddDays(1);

        var isOnLeave = await dbContext.DoctorDayOffs
            .AsNoTracking()
            .AnyAsync(ddo => ddo.DoctorId == cartItem.DoctorId &&
                             ddo.Date >= startOfDay &&
                             ddo.Date < endOfDay);
        if (isOnLeave)
        {
            throw new BadRequestException(ErrorCode.DOCTOR_ON_LEAVE);
        }

        var otherCartItems = await dbContext.CartItems
            .AsNoTracking()
            .Where(ci => ci.Id != cartItemId &&
                         ci.UserId == userId &&
                         ci.PatientProfileId == cartItem.PatientProfileId &&
                         ci.AppointmentDate == apptDateTime)
            .Select(ci => new { ci.DoctorId, ci.TimeSlot })
            .ToListAsync();

        foreach (var existing in otherCartItems)
        {
            if (existing.DoctorId == cartItem.DoctorId && existing.TimeSlot == cleanSlot)
            {
                throw new BadRequestException(ErrorCode.CART_ITEM_DUPLICATE);
            }

            if (TryParseSlot(existing.TimeSlot, out var exStart, out var exEnd) &&
                slotStart < exEnd && slotEnd > exStart)
            {
                throw new BadRequestException(ErrorCode.PATIENT_SCHEDULE_CONFLICT);
            }
        }

        cartItem.AppointmentDate = apptDateTime;
        cartItem.TimeSlot = cleanSlot;
        cartItem.Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();

        await dbContext.SaveChangesAsync();

        return await dbContext.CartItems
            .AsNoTracking()
            .Where(ci => ci.Id == cartItemId)
            .Select(ci => new CartItemResponse
            {
                Id = ci.Id,
                DoctorId = ci.DoctorId,
                DoctorFullName = ci.Doctor.FullName,
                DoctorAvatarUrl = ci.Doctor.AvatarUrl,
                DoctorIsActive = ci.Doctor.IsActive,
                DepartmentName = ci.Doctor.Department.Name,
                ConsultationFee = ci.Doctor.Department.ConsultationFee,
                PatientProfileId = ci.PatientProfileId,
                PatientProfileFullName = ci.PatientProfile.FullName,
                AppointmentDate = DateOnly.FromDateTime(ci.AppointmentDate),
                TimeSlot = ci.TimeSlot,
                Note = ci.Note,
                CreatedAt = ci.CreatedAt
            })
            .FirstAsync();
    }

    public async Task DeleteCartItemAsync(int userId, int cartItemId)
    {
        var cartItem = await dbContext.CartItems
            .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.UserId == userId);
        if (cartItem == null)
        {
            throw new NotFoundException(ErrorCode.RESOURCE_NOT_FOUND);
        }

        dbContext.CartItems.Remove(cartItem);
        await dbContext.SaveChangesAsync();
    }

    public async Task<CartCountResponse> GetCartCountAsync(int userId)
    {
        var count = await dbContext.CartItems
            .AsNoTracking()
            .CountAsync(ci => ci.UserId == userId);

        return new CartCountResponse { Count = count };
    }

    private static (TimeSpan Start, TimeSpan End) ParseAndValidateSlot(DateOnly appointmentDate, string timeSlot)
    {
        if (!TryParseSlot(timeSlot, out var start, out var end))
        {
            throw new BadRequestException(ErrorCode.INVALID_TIME_SLOT);
        }

        var nowVn = DateTime.UtcNow.AddHours(7);
        var todayVn = DateOnly.FromDateTime(nowVn);
        if (appointmentDate < todayVn || (appointmentDate == todayVn && start <= nowVn.TimeOfDay))
        {
            throw new BadRequestException(ErrorCode.APPOINTMENT_TIME_PASSED);
        }

        return (start, end);
    }

    private static bool TryParseSlot(string timeSlot, out TimeSpan start, out TimeSpan end)
    {
        start = default;
        end = default;
        var parts = timeSlot.Trim().Split('-');
        return parts.Length == 2 &&
               TimeSpan.TryParse(parts[0].Trim(), out start) &&
               TimeSpan.TryParse(parts[1].Trim(), out end) &&
               start < end;
    }
}
