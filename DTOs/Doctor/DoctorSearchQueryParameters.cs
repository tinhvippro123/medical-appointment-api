using System.ComponentModel.DataAnnotations;

namespace MedicalAppointment.API.DTOs.Doctor;

public class DoctorSearchQueryParameters
{
    [StringLength(100, ErrorMessage = "Từ khóa tìm kiếm tối đa 100 ký tự")]
    public string? Keyword { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Mã chuyên khoa phải lớn hơn 0")]
    public int? DepartmentId { get; set; }

    [Range(1, 100000, ErrorMessage = "Trang phải từ 1 đến 100000")]
    public int Page { get; set; } = 1;

    [Range(1, 50, ErrorMessage = "Số lượng mỗi trang phải từ 1 đến 50")]
    public int PageSize { get; set; } = 10;
}

