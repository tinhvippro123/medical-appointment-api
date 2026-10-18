using System.ComponentModel.DataAnnotations;

namespace MedicalAppointment.API.DTOs.Department;

public class DepartmentImageUrlDto
{
    [Required]
    public string ImageUrl { get; set; } = string.Empty;
}
