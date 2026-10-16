using System.ComponentModel.DataAnnotations;

namespace MedicalAppointment.API.DTOs.Doctor;

/// <summary>
/// Query parameters cho GET /api/doctors (danh sách phân trang).
/// Dùng [FromQuery] trên controller.
/// </summary>
public class DoctorQueryParameters
{
    [Range(1, int.MaxValue, ErrorMessage = "Trang phải lớn hơn 0")]
    public int Page { get; set; } = 1;

    [Range(1, 50, ErrorMessage = "Số lượng mỗi trang phải từ 1 đến 50")]
    public int PageSize { get; set; } = 10;
}
