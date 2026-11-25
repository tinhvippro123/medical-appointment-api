using System.ComponentModel.DataAnnotations;

namespace MedicalAppointment.API.DTOs;

public class VerifyEmailRequest
{
    [Required(ErrorMessage = "Vui lòng nhập địa chỉ email")]
    [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự")]
    [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mã OTP")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã OTP phải gồm đúng 6 chữ số")]
    public string Otp { get; set; } = string.Empty;
}

public class ResendVerificationRequest
{
    [Required(ErrorMessage = "Vui lòng nhập địa chỉ email")]
    [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự")]
    [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
    public string Email { get; set; } = string.Empty;
}
