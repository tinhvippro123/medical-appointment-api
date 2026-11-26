namespace MedicalAppointment.API.DTOs.Doctor;

public class DoctorScheduleItemResponse
{
    public int Id { get; set; }
    public int DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int MaxPatients { get; set; }
}
