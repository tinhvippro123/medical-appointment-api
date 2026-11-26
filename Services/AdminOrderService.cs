using MedicalAppointment.API.Data;
using MedicalAppointment.API.DTOs.Common;
using MedicalAppointment.API.DTOs.Order;
using MedicalAppointment.API.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace MedicalAppointment.API.Services;

public class AdminOrderService(ApplicationDbContext context) : IAdminOrderService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<PagedResult<AdminOrderResponse>> GetAllOrdersAsync(AdminOrderQueryParameters query)
    {
        if (query.FromDate.HasValue && query.ToDate.HasValue &&
            query.FromDate.Value.Date > query.ToDate.Value.Date)
        {
            throw new BadRequestException(ErrorCode.INVALID_DATE_RANGE);
        }

        var baseQuery = _context.Orders.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var filterStatus = query.Status.Trim();
            baseQuery = baseQuery.Where(o => o.Status == filterStatus);
        }

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword.Trim();
            if (keyword.StartsWith('#') && int.TryParse(keyword[1..], out var exactOrderId))
            {
                baseQuery = baseQuery.Where(o => o.Id == exactOrderId);
            }
            else
            {
                var hasOrderId = int.TryParse(keyword, out var orderId);
                baseQuery = baseQuery.Where(o =>
                    o.User.FullName.Contains(keyword) ||
                    o.User.Phone.Contains(keyword) ||
                    (hasOrderId && o.Id == orderId));
            }
        }

        // Quy đổi khoảng ngày Việt Nam (UTC+7) sang mốc UTC lưu trong CreatedAt
        if (query.FromDate.HasValue)
        {
            var fromUtc = DateTime.SpecifyKind(query.FromDate.Value.Date.AddHours(-7), DateTimeKind.Utc);
            baseQuery = baseQuery.Where(o => o.CreatedAt >= fromUtc);
        }

        if (query.ToDate.HasValue)
        {
            var toUtcExclusive = DateTime.SpecifyKind(query.ToDate.Value.Date.AddDays(1).AddHours(-7), DateTimeKind.Utc);
            baseQuery = baseQuery.Where(o => o.CreatedAt < toUtcExclusive);
        }

        var totalCount = await baseQuery.CountAsync();

        var items = await baseQuery
            .OrderByDescending(o => o.CreatedAt)
            .ThenByDescending(o => o.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(o => new AdminOrderResponse
            {
                Id = o.Id,
                UserId = o.UserId,
                UserFullName = o.User.FullName,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                CancellationReason = o.CancellationReason,
                PaymentMethod = o.PaymentMethod,
                PaymentStatus = o.PaymentStatus,
                TransactionId = o.TransactionId,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt
            })
            .ToListAsync();

        return new PagedResult<AdminOrderResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<AdminOrderDetailResponse> GetOrderByIdAsync(int id)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Where(o => o.Id == id)
            .Select(o => new AdminOrderDetailResponse
            {
                Id = o.Id,
                UserId = o.UserId,
                UserFullName = o.User.FullName,
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
                    .Select(od => new AdminOrderItemResponse
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

    public async Task<AdminOrderDetailResponse> UpdateOrderStatusAsync(int id, UpdateOrderStatusRequest request)
    {
        var order = await _context.Orders
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            throw new NotFoundException(ErrorCode.ORDER_NOT_FOUND);
        }

        var currentStatus = order.Status.Trim();
        var newStatus = request.Status.Trim();

        bool isValidTransition = (currentStatus, newStatus) switch
        {
            ("Pending", "Confirmed") => true,
            ("Pending", "Cancelled") => true,
            ("Confirmed", "Completed") => true,
            ("Confirmed", "Cancelled") => true,
            _ => false
        };

        if (!isValidTransition)
        {
            throw new BadRequestException(ErrorCode.INVALID_ORDER_STATUS_TRANSITION);
        }

        order.Status = newStatus;

        if (newStatus == "Cancelled")
        {
            order.CancellationReason = string.IsNullOrWhiteSpace(request.CancellationReason)
                ? null
                : request.CancellationReason.Trim();
        }

        foreach (var detail in order.OrderDetails)
        {
            detail.Status = newStatus;
        }

        await _context.SaveChangesAsync();

        return await GetOrderByIdAsync(id);
    }
}
