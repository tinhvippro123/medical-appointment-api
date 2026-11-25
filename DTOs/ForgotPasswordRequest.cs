using System.ComponentModel.DataAnnotations;

namespace MedicalAppointment.API.DTOs;

public class ForgotPasswordRequest
{
    [Required(ErrorMessage = "Vui lòng nhập địa chỉ email")]
    [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự")]
    [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
    public string Email { get; set; } = string.Empty;
}
