namespace MedicalAppointment.API.DTOs.Doctor;

/// <summary>
/// DTO đại diện cho một khung lịch làm việc trong chi tiết bác sĩ.
/// </summary>
public class DoctorScheduleItemResponse
{
    public int Id { get; set; }
    public int DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int MaxPatients { get; set; }
}
