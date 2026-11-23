using System.ComponentModel.DataAnnotations;

namespace MedicalAppointment.API.DTOs.Role;

public class CreateRoleRequest
{
    [Required(ErrorMessage = "Vui lòng nhập mã vai trò")]
    [StringLength(30, MinimumLength = 2, ErrorMessage = "Mã vai trò phải từ 2 đến 30 ký tự")]
    [RegularExpression("^[A-Za-z0-9_]+$", ErrorMessage = "Mã vai trò chỉ gồm chữ, số và dấu gạch dưới (_)")]
    public string Key { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập tên vai trò")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Tên vai trò phải từ 2 đến 50 ký tự")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200, ErrorMessage = "Mô tả tối đa 200 ký tự")]
    public string? Description { get; set; }
}
