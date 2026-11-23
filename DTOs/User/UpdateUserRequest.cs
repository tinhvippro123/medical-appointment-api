using System.ComponentModel.DataAnnotations;

namespace MedicalAppointment.API.DTOs.User;

public class UpdateUserRequest
{
    [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
    [StringLength(40, MinimumLength = 2, ErrorMessage = "Họ và tên phải từ 2 đến 40 ký tự")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải gồm 10 chữ số và bắt đầu bằng số 0")]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    [MaxLength(100, ErrorMessage = "Email tối đa 100 ký tự")]
    public string? Email { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn vai trò hợp lệ")]
    public int RoleId { get; set; }

    public bool IsActive { get; set; } = true;
}
