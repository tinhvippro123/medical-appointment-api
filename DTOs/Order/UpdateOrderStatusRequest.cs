using System.ComponentModel.DataAnnotations;

namespace MedicalAppointment.API.DTOs.Order;

public class UpdateOrderStatusRequest
{
    [Required(ErrorMessage = "Trạng thái không được để trống")]
    [RegularExpression("^(Pending|Confirmed|Completed|Cancelled)$", ErrorMessage = "Trạng thái không hợp lệ")]
    public string Status { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Lý do hủy không được vượt quá 500 ký tự")]
    public string? CancellationReason { get; set; }
}
