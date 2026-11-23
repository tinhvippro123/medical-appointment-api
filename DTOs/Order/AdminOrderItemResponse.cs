namespace MedicalAppointment.API.DTOs.Order;

public class AdminOrderItemResponse
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int DoctorId { get; set; }
    public string DoctorFullName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string? RoomNumber { get; set; }
    public int PatientProfileId { get; set; }
    public string PatientProfileFullName { get; set; } = string.Empty;
    public DateOnly AppointmentDate { get; set; }
    public string TimeSlot { get; set; } = string.Empty;
    public int QueueNumber { get; set; }
    public decimal Fee { get; set; }
    public string? Note { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
