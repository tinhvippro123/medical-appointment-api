namespace MedicalAppointment.API.DTOs.Doctor;

/// <summary>
/// Response DTO cho thông tin bác sĩ.
/// Thiết kế mở rộng được: B3 (Doctor Detail) có thể bổ sung Schedules, DayOffs
/// mà không đổi hợp đồng hiện tại.
/// Room null (bác sĩ chưa gán phòng) thì RoomNumber và Floor trả null.
/// </summary>
public class DoctorResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Qualification { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int? RoomId { get; set; }
    public string? RoomNumber { get; set; }
    public int? Floor { get; set; }
    public string? Description { get; set; }
    public string? AvatarUrl { get; set; }
    public int ExperienceYears { get; set; }
    public decimal ConsultationFee { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
