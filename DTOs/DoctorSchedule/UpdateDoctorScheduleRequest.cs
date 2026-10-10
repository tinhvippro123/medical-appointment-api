namespace MedicalAppointment.API.DTOs.DoctorSchedule;

public class UpdateDoctorScheduleRequest
{
    public required int DayOfWeek { get; set; }
    public required TimeSpan StartTime { get; set; }
    public required TimeSpan EndTime { get; set; }
    public int? MaxPatients { get; set; }
}
