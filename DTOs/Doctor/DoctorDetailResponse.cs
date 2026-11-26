namespace MedicalAppointment.API.DTOs.Doctor;

/// <summary>
/// Response DTO cho chi tiết 1 bác sĩ (mở rộng từ DoctorResponse của B1).
/// Giữ nguyên toàn bộ trường hiện có của DoctorResponse, bổ sung danh sách lịch làm việc (Schedules).
/// </summary>
public class DoctorDetailResponse : DoctorResponse
{
    public List<DoctorScheduleItemResponse> Schedules { get; set; } = [];
}
