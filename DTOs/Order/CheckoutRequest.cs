using System.ComponentModel.DataAnnotations;
using MedicalAppointment.API.DTOs.Cart;

namespace MedicalAppointment.API.DTOs.Order;

public class CheckoutRequest
{
    [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
    [RegularExpression("^(Cash|VNPay|VietQR)$", ErrorMessage = "Phương thức thanh toán không hợp lệ (chỉ chấp nhận Cash, VNPay, VietQR)")]
    public string PaymentMethod { get; set; } = string.Empty;

    public AddCartItemRequest? Appointment { get; set; }
}
