using MedicalAppointment.API.Data;
using MedicalAppointment.API.DTOs.Cart;
using MedicalAppointment.API.Entities;
using MedicalAppointment.API.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace MedicalAppointment.API.Services;

public class CartService(ApplicationDbContext dbContext) : ICartService
{
    public async Task<CartItemResponse> AddToCartAsync(int userId, AddCartItemRequest request)
    {
        var doctorExists = await dbContext.Doctors
            .AsNoTracking()
            .AnyAsync(d => d.Id == request.DoctorId!.Value && d.IsActive);
        if (!doctorExists)
        {
            throw new BadRequestException(ErrorCode.DOCTOR_NOT_FOUND);
        }

        var profileExists = await dbContext.PatientProfiles
            .AsNoTracking()
            .AnyAsync(p => p.Id == request.PatientProfileId!.Value && p.UserId == userId);
        if (!profileExists)
        {
            throw new BadRequestException(ErrorCode.INVALID_PATIENT_PROFILE);
        }

        var appointmentDate = request.AppointmentDate!.Value;
        var dayOfWeek = (int)appointmentDate.DayOfWeek;
        var cleanSlot = request.TimeSlot.Trim();
        var parts = cleanSlot.Split('-');
        var slotStart = TimeSpan.Parse(parts[0]);
        var slotEnd = TimeSpan.Parse(parts[1]);

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
        var isDuplicate = await dbContext.CartItems
            .AsNoTracking()
            .AnyAsync(ci => ci.UserId == userId &&
                            ci.DoctorId == request.DoctorId!.Value &&
                            ci.PatientProfileId == request.PatientProfileId!.Value &&
                            ci.AppointmentDate == apptDateTime &&
                            ci.TimeSlot == cleanSlot);
        if (isDuplicate)
        {
            throw new BadRequestException(ErrorCode.CART_ITEM_DUPLICATE);
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
                ConsultationFee = ci.Doctor.ConsultationFee,
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
                ConsultationFee = ci.Doctor.ConsultationFee,
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
            .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.UserId == userId);
        if (cartItem == null)
        {
            throw new NotFoundException(ErrorCode.RESOURCE_NOT_FOUND);
        }

        if (!cartItem.Doctor.IsActive)
        {
            throw new BadRequestException(ErrorCode.DOCTOR_NOT_FOUND);
        }

        var appointmentDate = request.AppointmentDate!.Value;
        var dayOfWeek = (int)appointmentDate.DayOfWeek;
        var cleanSlot = request.TimeSlot.Trim();
        var parts = cleanSlot.Split('-');
        var slotStart = TimeSpan.Parse(parts[0]);
        var slotEnd = TimeSpan.Parse(parts[1]);

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
        var isDuplicate = await dbContext.CartItems
            .AsNoTracking()
            .AnyAsync(ci => ci.Id != cartItemId &&
                            ci.UserId == userId &&
                            ci.DoctorId == cartItem.DoctorId &&
                            ci.PatientProfileId == cartItem.PatientProfileId &&
                            ci.AppointmentDate == apptDateTime &&
                            ci.TimeSlot == cleanSlot);
        if (isDuplicate)
        {
            throw new BadRequestException(ErrorCode.CART_ITEM_DUPLICATE);
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
                ConsultationFee = ci.Doctor.ConsultationFee,
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
}
