using System.ComponentModel.DataAnnotations;

namespace MedicalAppointment.API.DTOs.DoctorDayOff;

public class CreateDoctorDayOffRequest
{
    [Required(ErrorMessage = "Vui lòng chọn ngày nghỉ.")]
    public DateOnly? Date { get; set; }

    [StringLength(200, ErrorMessage = "Lý do không được vượt quá 200 ký tự.")]
    public string? Reason { get; set; }
}
