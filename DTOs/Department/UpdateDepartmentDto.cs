using System.ComponentModel.DataAnnotations;

namespace MedicalAppointment.API.DTOs.Department;

public class UpdateDepartmentDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập phí khám")]
    [Range(typeof(decimal), "0.01", "9999999999999999.99", ErrorMessage = "Phí khám phải lớn hơn 0")]
    public decimal? ConsultationFee { get; set; }
}

