using System.ComponentModel.DataAnnotations;

namespace MedicalAppointment.API.DTOs;

public class ResetPasswordRequest
{
    [Required(ErrorMessage = "Vui lòng nhập địa chỉ email")]
    [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự")]
    [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mã OTP")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã OTP phải gồm đúng 6 chữ số")]
    public string Otp { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
    [StringLength(50, MinimumLength = 6, ErrorMessage = "Mật khẩu mới phải từ 6 đến 50 ký tự")]
    public string NewPassword { get; set; } = string.Empty;
}
