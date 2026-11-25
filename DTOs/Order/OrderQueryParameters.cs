using System.ComponentModel.DataAnnotations;

namespace MedicalAppointment.API.DTOs.Order;

public class OrderQueryParameters
{
    [Range(1, 100000, ErrorMessage = "Trang phải từ 1 đến 100000")]
    public int Page { get; set; } = 1;

    [Range(1, 50, ErrorMessage = "Số lượng mỗi trang phải từ 1 đến 50")]
    public int PageSize { get; set; } = 10;

    [RegularExpression("^(Pending|Confirmed|Completed|Cancelled)$", ErrorMessage = "Trạng thái lọc không hợp lệ")]
    public string? Status { get; set; }

    [MaxLength(100, ErrorMessage = "Từ khóa tìm kiếm tối đa 100 ký tự")]
    public string? Search { get; set; }
}
