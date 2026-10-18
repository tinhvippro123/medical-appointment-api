namespace MedicalAppointment.API.DTOs.DoctorSchedule;

public class DoctorScheduleResponse
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public int DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int MaxPatients { get; set; }
}
