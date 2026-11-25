using MedicalAppointment.API.DTOs.Payment;

namespace MedicalAppointment.API.Services.Interface;

public interface IPaymentService
{
    Task<VnPayPaymentResponse> CreateVnPayUrlAsync(int userId, bool isAdmin, int orderId, string clientIp, string baseCallbackUrl);
    Task<VietQrPaymentResponse> CreateVietQrAsync(int userId, bool isAdmin, int orderId);
    Task<PaymentCallbackResponse> HandleVnPayCallbackAsync(IQueryCollection query);
    Task<PaymentCallbackResponse> ConfirmPaymentAsync(int userId, bool isAdmin, ConfirmPaymentRequest request);
}
