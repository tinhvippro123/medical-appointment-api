using System.ComponentModel.DataAnnotations;

namespace MedicalAppointment.API.DTOs.User;

public class UserQueryParameters
{
    [Range(1, int.MaxValue, ErrorMessage = "Trang phải lớn hơn 0")]
    public int Page { get; set; } = 1;

    [Range(1, 50, ErrorMessage = "Số lượng mỗi trang phải từ 1 đến 50")]
    public int PageSize { get; set; } = 10;

    public string? Keyword { get; set; }
    public int? RoleId { get; set; }
    public bool? IsActive { get; set; }
}
