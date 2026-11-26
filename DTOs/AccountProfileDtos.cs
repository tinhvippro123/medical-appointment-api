using System.ComponentModel.DataAnnotations;

namespace MedicalAppointment.API.DTOs;

public class VerifyPasswordRequest
{
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu xác thực")]
    public string Password { get; set; } = string.Empty;
}

public class UpdateMyProfileRequest
{
    [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
    [StringLength(40, MinimumLength = 2, ErrorMessage = "Họ và tên phải từ 2 đến 40 ký tự")]
    public string FullName { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự")]
    [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
    public string? Email { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [StringLength(10, ErrorMessage = "Giới tính tối đa 10 ký tự")]
    public string? Gender { get; set; }
}

public class MyProfileResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string RoleKey { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
}

public class UpdateMyProfileResponse
{
    public string Message { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public MyProfileResponse Profile { get; set; } = null!;
}
