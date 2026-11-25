using System.ComponentModel.DataAnnotations;

namespace MedicalAppointment.API.DTOs.Payment;

public class CreatePaymentRequest
{
    [Required(ErrorMessage = "Vui lòng truyền mã đơn đặt lịch")]
    [Range(1, int.MaxValue, ErrorMessage = "Mã đơn đặt lịch không hợp lệ")]
    public int OrderId { get; set; }
}

public class ConfirmPaymentRequest
{
    [Required(ErrorMessage = "Vui lòng truyền mã đơn đặt lịch")]
    [Range(1, int.MaxValue, ErrorMessage = "Mã đơn đặt lịch không hợp lệ")]
    public int OrderId { get; set; }

    [RegularExpression("^(VNPay|VietQR)$", ErrorMessage = "Cổng thanh toán chỉ chấp nhận VNPay hoặc VietQR")]
    public string Provider { get; set; } = "VietQR";

    public bool SimulateFailure { get; set; }
}

public class VnPayPaymentResponse
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string TxnRef { get; set; } = string.Empty;
    public string PaymentUrl { get; set; } = string.Empty;
    public string CallbackSuccessUrl { get; set; } = string.Empty;
    public string CallbackCancelUrl { get; set; } = string.Empty;
}

public class VietQrPaymentResponse
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string TxnRef { get; set; } = string.Empty;
    public string BankId { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string AccountNo { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string TransferContent { get; set; } = string.Empty;
    public string QrData { get; set; } = string.Empty;
    public string VietQrImageUrl { get; set; } = string.Empty;
}

public class PaymentCallbackResponse
{
    public bool Success { get; set; }
    public int OrderId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public string ResponseCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
