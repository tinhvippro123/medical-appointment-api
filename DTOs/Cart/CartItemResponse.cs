namespace MedicalAppointment.API.DTOs.Cart;

public class CartItemResponse
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public string DoctorFullName { get; set; } = string.Empty;
    public string? DoctorAvatarUrl { get; set; }
    public bool DoctorIsActive { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public decimal ConsultationFee { get; set; }
    public int PatientProfileId { get; set; }
    public string PatientProfileFullName { get; set; } = string.Empty;
    public DateOnly AppointmentDate { get; set; }
    public string TimeSlot { get; set; } = string.Empty;
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}
