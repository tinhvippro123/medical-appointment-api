namespace MedicalAppointment.API.DTOs.Doctor;

public class DoctorDetailResponse : DoctorResponse
{
    public List<DoctorScheduleItemResponse> Schedules { get; set; } = [];
}
