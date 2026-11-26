using System.Data;
using MedicalAppointment.API.Data;
using MedicalAppointment.API.DTOs.Common;
using MedicalAppointment.API.DTOs.Order;
using MedicalAppointment.API.Entities;
using MedicalAppointment.API.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MedicalAppointment.API.Services;

public class OrderService(ApplicationDbContext dbContext) : IOrderService
{
    private const int MaxRetries = 2; // Thử lại tối đa 2 lần khi deadlock (tổng cộng 3 lượt thực thi)

    public async Task<OrderDetailResponse> CheckoutAsync(int userId, CheckoutRequest request)
    {
        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            dbContext.ChangeTracker.Clear();

            try
            {
                return await ExecuteCheckoutAttemptAsync(userId, request);
            }
            catch (Exception ex) when (IsTransientConflict(ex) && attempt < MaxRetries)
            {
                // Jittered backoff trước khi thử lại
                await Task.Delay(50 * (attempt + 1));
            }
            catch (Exception ex) when (IsTransientConflict(ex) && attempt >= MaxRetries)
            {
                throw new BadRequestException(ErrorCode.CHECKOUT_CONFLICT);
            }
        }

        throw new BadRequestException(ErrorCode.CHECKOUT_CONFLICT);
    }

    private async Task<OrderDetailResponse> ExecuteCheckoutAttemptAsync(int userId, CheckoutRequest request)
    {
        // Mở transaction Serializable trước khi đọc dữ liệu tránh race condition cấp STT
        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        List<CartItem> cartItems;
        var isDirectAppointment = request.Appointment != null;

        if (isDirectAppointment)
        {
            var appt = request.Appointment!;
            var doctor = await dbContext.Doctors
                .Include(d => d.Department)
                .FirstOrDefaultAsync(d => d.Id == appt.DoctorId!.Value);
            var profile = await dbContext.PatientProfiles
                .FirstOrDefaultAsync(p => p.Id == appt.PatientProfileId!.Value);

            cartItems =
            [
                new CartItem
                {
                    Id = 0,
                    UserId = userId,
                    DoctorId = appt.DoctorId!.Value,
                    Doctor = doctor!,
                    PatientProfileId = appt.PatientProfileId!.Value,
                    PatientProfile = profile!,
                    AppointmentDate = appt.AppointmentDate!.Value.ToDateTime(TimeOnly.MinValue),
                    TimeSlot = appt.TimeSlot.Trim(),
                    Note = string.IsNullOrWhiteSpace(appt.Note) ? null : appt.Note.Trim()
                }
            ];
        }
        else
        {
            cartItems = await dbContext.CartItems
                .Include(ci => ci.Doctor).ThenInclude(d => d.Department)
                .Include(ci => ci.PatientProfile)
                .Where(ci => ci.UserId == userId)
                .OrderBy(ci => ci.Id)
                .ToListAsync();
        }

        if (cartItems.Count == 0)
        {
            throw new BadRequestException(ErrorCode.CART_EMPTY);
        }

        // Tính ngày giờ Việt Nam (UTC+7)
        var nowVn = DateTime.UtcNow.AddHours(7);
        var todayVn = DateOnly.FromDateTime(nowVn);
        var currentTimeOfDay = nowVn.TimeOfDay;

        // Lưu lại ca làm việc tương ứng của từng mục để kiểm tra sức chứa
        var scheduleMap = new Dictionary<int, DoctorSchedule>();

        foreach (var item in cartItems)
        {
            // Bác sĩ và chuyên khoa tồn tại và còn hoạt động
            if (item.Doctor == null || !item.Doctor.IsActive || item.Doctor.Department == null || !item.Doctor.Department.IsActive)
            {
                throw new BadRequestException(ErrorCode.DOCTOR_UNAVAILABLE);
            }

            // Hồ sơ bệnh nhân tồn tại và thuộc quyền sở hữu của user
            if (item.PatientProfile == null || item.PatientProfile.UserId != userId)
            {
                throw new BadRequestException(ErrorCode.INVALID_PATIENT_PROFILE);
            }

            // Ngày khám không ở quá khứ
            var apptDateOnly = DateOnly.FromDateTime(item.AppointmentDate);
            if (apptDateOnly < todayVn)
            {
                throw new BadRequestException(ErrorCode.APPOINTMENT_TIME_PASSED);
            }

            // Giờ khám không ở quá khứ (nếu là hôm nay)
            var (slotStart, slotEnd) = ParseSlot(item.TimeSlot);
            if (apptDateOnly == todayVn && slotStart <= currentTimeOfDay)
            {
                throw new BadRequestException(ErrorCode.APPOINTMENT_TIME_PASSED);
            }

            // Bác sĩ không nghỉ ngày đó (dùng khoảng [ngày, ngày + 1 ngày))
            var startOfDay = item.AppointmentDate.Date;
            var endOfDay = startOfDay.AddDays(1);

            var isOnLeave = await dbContext.DoctorDayOffs
                .AsNoTracking()
                .AnyAsync(ddo => ddo.DoctorId == item.DoctorId &&
                                 ddo.Date >= startOfDay &&
                                 ddo.Date < endOfDay);
            if (isOnLeave)
            {
                throw new BadRequestException(ErrorCode.DOCTOR_ON_LEAVE);
            }

            // Khung giờ còn nằm trong lịch làm việc của bác sĩ
            var dayOfWeek = (int)item.AppointmentDate.DayOfWeek;
            var schedule = await dbContext.DoctorSchedules
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.DoctorId == item.DoctorId &&
                                          s.DayOfWeek == dayOfWeek &&
                                          s.StartTime <= slotStart &&
                                          s.EndTime >= slotEnd);
            if (schedule == null)
            {
                throw new BadRequestException(ErrorCode.INVALID_TIME_SLOT);
            }

            scheduleMap[item.Id] = schedule;
        }

        // Kiểm tra trùng khung giờ của cùng hồ sơ bệnh nhân trong giỏ hàng
        for (int i = 0; i < cartItems.Count; i++)
        {
            var itemA = cartItems[i];
            var (startA, endA) = ParseSlot(itemA.TimeSlot);

            for (int j = i + 1; j < cartItems.Count; j++)
            {
                var itemB = cartItems[j];
                if (itemA.PatientProfileId == itemB.PatientProfileId &&
                    itemA.AppointmentDate.Date == itemB.AppointmentDate.Date)
                {
                    var (startB, endB) = ParseSlot(itemB.TimeSlot);
                    if (SlotsOverlap(startA, endA, startB, endB))
                    {
                        throw new BadRequestException(ErrorCode.PATIENT_SCHEDULE_CONFLICT);
                    }
                }
            }
        }

        // Kiểm tra trùng với các lịch đã đặt trong DB
        foreach (var item in cartItems)
        {
            var (slotStart, slotEnd) = ParseSlot(item.TimeSlot);
            var startOfDay = item.AppointmentDate.Date;
            var endOfDay = startOfDay.AddDays(1);

            var existingPatientSlots = await dbContext.OrderDetails
                .AsNoTracking()
                .Where(od => od.PatientProfileId == item.PatientProfileId &&
                             od.AppointmentDate >= startOfDay &&
                             od.AppointmentDate < endOfDay &&
                             od.Status != "Cancelled")
                .Select(od => od.TimeSlot)
                .ToListAsync();

            foreach (var existingSlot in existingPatientSlots)
            {
                var (exStart, exEnd) = ParseSlot(existingSlot);
                if (SlotsOverlap(slotStart, slotEnd, exStart, exEnd))
                {
                    throw new BadRequestException(ErrorCode.PATIENT_SCHEDULE_CONFLICT);
                }
            }
        }

        // Kiểm tra sức chứa (MaxPatients) theo khoảng giờ chồng nhau
        for (int i = 0; i < cartItems.Count; i++)
        {
            var item = cartItems[i];
            var schedule = scheduleMap[item.Id];
            var (slotStart, slotEnd) = ParseSlot(item.TimeSlot);
            var startOfDay = item.AppointmentDate.Date;
            var endOfDay = startOfDay.AddDays(1);

            // Tải danh sách TimeSlot của bác sĩ trong ngày từ các OrderDetails có trạng thái khác Cancelled
            var existingDoctorSlots = await dbContext.OrderDetails
                .AsNoTracking()
                .Where(od => od.DoctorId == item.DoctorId &&
                             od.AppointmentDate >= startOfDay &&
                             od.AppointmentDate < endOfDay &&
                             od.Status != "Cancelled")
                .Select(od => od.TimeSlot)
                .ToListAsync();

            int overlappingCount = 0;
            foreach (var exSlot in existingDoctorSlots)
            {
                var (exStart, exEnd) = ParseSlot(exSlot);
                if (SlotsOverlap(slotStart, slotEnd, exStart, exEnd))
                {
                    overlappingCount++;
                }
            }

            // Đếm thêm các item đứng trước trong giỏ hàng hiện tại có giờ chồng nhau
            for (int k = 0; k < i; k++)
            {
                var prevItem = cartItems[k];
                if (prevItem.DoctorId == item.DoctorId &&
                    prevItem.AppointmentDate.Date == item.AppointmentDate.Date)
                {
                    var (prevStart, prevEnd) = ParseSlot(prevItem.TimeSlot);
                    if (SlotsOverlap(slotStart, slotEnd, prevStart, prevEnd))
                    {
                        overlappingCount++;
                    }
                }
            }

            if (overlappingCount >= schedule.MaxPatients)
            {
                throw new BadRequestException(ErrorCode.TIME_SLOT_FULL);
            }
        }

        // Cấp số thứ tự theo (DoctorId, ngày khám), tính trên mọi trạng thái kể cả Cancelled
        var queueTracker = new Dictionary<(int DoctorId, DateTime Date), int>();
        var assignedQueueNumbers = new Dictionary<int, int>();

        foreach (var item in cartItems)
        {
            var key = (item.DoctorId, item.AppointmentDate.Date);
            if (!queueTracker.TryGetValue(key, out var currentMaxQueue))
            {
                var startOfDay = item.AppointmentDate.Date;
                var endOfDay = startOfDay.AddDays(1);

                currentMaxQueue = await dbContext.OrderDetails
                    .Where(od => od.DoctorId == item.DoctorId &&
                                 od.AppointmentDate >= startOfDay &&
                                 od.AppointmentDate < endOfDay)
                    .MaxAsync(od => (int?)od.QueueNumber) ?? 0;
            }

            currentMaxQueue++;
            queueTracker[key] = currentMaxQueue;
            assignedQueueNumbers[item.Id] = currentMaxQueue;
        }

        var totalAmount = cartItems.Sum(ci => ci.Doctor.Department.ConsultationFee);

        var order = new Order
        {
            UserId = userId,
            TotalAmount = totalAmount,
            Status = "Pending",
            CancellationReason = null,
            PaymentMethod = request.PaymentMethod,
            PaymentStatus = "Unpaid",
            TransactionId = null
        };

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();

        var orderDetails = cartItems.Select(ci => new OrderDetail
        {
            OrderId = order.Id,
            DoctorId = ci.DoctorId,
            PatientProfileId = ci.PatientProfileId,
            AppointmentDate = ci.AppointmentDate,
            TimeSlot = ci.TimeSlot,
            QueueNumber = assignedQueueNumbers[ci.Id],
            Fee = ci.Doctor.Department.ConsultationFee,
            Note = ci.Note,
            Status = "Pending"
        }).ToList();

        dbContext.OrderDetails.AddRange(orderDetails);
        if (!isDirectAppointment)
        {
            dbContext.CartItems.RemoveRange(cartItems);
        }

        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return await GetOrderByIdInternalAsync(userId, order.Id);
    }

    public async Task<PagedResult<OrderResponse>> GetOrdersAsync(int userId, OrderQueryParameters query)
    {
        var baseQuery = dbContext.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId);

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var filterStatus = query.Status.Trim();
            baseQuery = baseQuery.Where(o => o.Status == filterStatus);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var keyword = query.Search.Trim();
            var idCandidate = keyword.TrimStart('#');
            var hasOrderId = int.TryParse(idCandidate, out var parsedOrderId);

            baseQuery = baseQuery.Where(o =>
                (hasOrderId && o.Id == parsedOrderId) ||
                (o.TransactionId != null && o.TransactionId.Contains(keyword)) ||
                o.PaymentMethod.Contains(keyword) ||
                o.OrderDetails.Any(od =>
                    od.Doctor.FullName.Contains(keyword) ||
                    od.Doctor.Department.Name.Contains(keyword) ||
                    od.PatientProfile.FullName.Contains(keyword) ||
                    (od.Doctor.Room != null && od.Doctor.Room.RoomNumber.Contains(keyword))));
        }

        var totalCount = await baseQuery.CountAsync();

        var items = await baseQuery
            .OrderByDescending(o => o.CreatedAt)
            .ThenByDescending(o => o.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(o => new OrderResponse
            {
                Id = o.Id,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                CancellationReason = o.CancellationReason,
                PaymentMethod = o.PaymentMethod,
                PaymentStatus = o.PaymentStatus,
                TransactionId = o.TransactionId,
                TotalItems = o.OrderDetails.Count,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt
            })
            .ToListAsync();

        return new PagedResult<OrderResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<OrderDetailResponse> GetOrderByIdAsync(int userId, int orderId)
    {
        return await GetOrderByIdInternalAsync(userId, orderId);
    }

    public async Task DiscardUnpaidOrderAsync(int userId, int orderId)
    {
        var order = await dbContext.Orders
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null)
        {
            return;
        }

        if (string.Equals(order.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException(ErrorCode.ORDER_ALREADY_PAID);
        }

        if (!string.Equals(order.Status, "Pending", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(order.PaymentMethod, "Cash", StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException(ErrorCode.CANNOT_DISCARD_ORDER);
        }

        dbContext.OrderDetails.RemoveRange(order.OrderDetails);
        dbContext.Orders.Remove(order);
        await dbContext.SaveChangesAsync();
    }

    private async Task<OrderDetailResponse> GetOrderByIdInternalAsync(int userId, int orderId)
    {
        var order = await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.Id == orderId && o.UserId == userId)
            .Select(o => new OrderDetailResponse
            {
                Id = o.Id,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                CancellationReason = o.CancellationReason,
                PaymentMethod = o.PaymentMethod,
                PaymentStatus = o.PaymentStatus,
                TransactionId = o.TransactionId,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt,
                OrderDetails = o.OrderDetails
                    .OrderBy(od => od.QueueNumber)
                    .ThenBy(od => od.Id)
                    .Select(od => new OrderItemResponse
                    {
                        Id = od.Id,
                        OrderId = od.OrderId,
                        DoctorId = od.DoctorId,
                        DoctorFullName = od.Doctor.FullName,
                        DepartmentName = od.Doctor.Department.Name,
                        RoomNumber = od.Doctor.Room != null ? od.Doctor.Room.RoomNumber : null,
                        PatientProfileId = od.PatientProfileId,
                        PatientProfileFullName = od.PatientProfile.FullName,
                        AppointmentDate = DateOnly.FromDateTime(od.AppointmentDate),
                        TimeSlot = od.TimeSlot,
                        QueueNumber = od.QueueNumber,
                        Fee = od.Fee,
                        Note = od.Note,
                        Status = od.Status,
                        CreatedAt = od.CreatedAt
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (order == null)
        {
            throw new NotFoundException(ErrorCode.ORDER_NOT_FOUND);
        }

        return order;
    }

    private static (TimeSpan Start, TimeSpan End) ParseSlot(string timeSlot)
    {
        var parts = timeSlot.Trim().Split('-');
        if (parts.Length != 2 ||
            !TimeSpan.TryParse(parts[0].Trim(), out var start) ||
            !TimeSpan.TryParse(parts[1].Trim(), out var end) ||
            start >= end)
        {
            throw new BadRequestException(ErrorCode.INVALID_TIME_SLOT);
        }
        return (start, end);
    }

    private static bool SlotsOverlap(TimeSpan startA, TimeSpan endA, TimeSpan startB, TimeSpan endB)
    {
        return startA < endB && endA > startB;
    }

    private static bool IsTransientConflict(Exception ex)
    {
        if (ex is DbUpdateConcurrencyException)
        {
            return true;
        }

        if (ex is SqlException sqlEx && sqlEx.Number == 1205)
        {
            return true;
        }

        if (ex.InnerException != null)
        {
            return IsTransientConflict(ex.InnerException);
        }

        return false;
    }
}
