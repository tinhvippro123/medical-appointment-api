using System.ComponentModel.DataAnnotations;

namespace MedicalAppointment.API.DTOs;

public class LoginRequest
{
    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    public string Password { get; set; } = string.Empty;
}