using MedicalAppointment.API.DTOs.Common;
using MedicalAppointment.API.DTOs.Order;

namespace MedicalAppointment.API.Services;

public interface IOrderService
{
    Task<OrderDetailResponse> CheckoutAsync(int userId, CheckoutRequest request);
    Task<PagedResult<OrderResponse>> GetOrdersAsync(int userId, OrderQueryParameters query);
    Task<OrderDetailResponse> GetOrderByIdAsync(int userId, int orderId);
    Task DiscardUnpaidOrderAsync(int userId, int orderId);
}
