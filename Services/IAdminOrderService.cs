using MedicalAppointment.API.DTOs.Common;
using MedicalAppointment.API.DTOs.Order;

namespace MedicalAppointment.API.Services;

public interface IAdminOrderService
{
    Task<PagedResult<AdminOrderResponse>> GetAllOrdersAsync(AdminOrderQueryParameters query);
    Task<AdminOrderDetailResponse> GetOrderByIdAsync(int id);
    Task<AdminOrderDetailResponse> UpdateOrderStatusAsync(int id, UpdateOrderStatusRequest request);
}
