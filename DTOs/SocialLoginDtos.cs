using System.ComponentModel.DataAnnotations;

namespace MedicalAppointment.API.DTOs;

public class SocialLoginRequest
{
    [Required(ErrorMessage = "Vui lòng chọn phương thức đăng nhập (Google, Facebook, Zalo)")]
    [StringLength(20)]
    public string Provider { get; set; } = string.Empty;

    [Required(ErrorMessage = "Thiếu mã định danh tài khoản mạng xã hội")]
    [StringLength(128)]
    public string ProviderUserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập tên hiển thị")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Tên hiển thị phải từ 2 đến 80 ký tự")]
    public string FullName { get; set; } = string.Empty;

    [StringLength(150)]
    [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
    public string? Email { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(500)]
    public string? AvatarUrl { get; set; }
}

public class SocialLoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public bool IsNewUser { get; set; }
}
