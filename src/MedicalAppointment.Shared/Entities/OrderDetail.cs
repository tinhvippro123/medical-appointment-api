namespace MedicalAppointment.Shared.Entities
{
    public class OrderDetail : BaseEntity
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int DoctorId { get; set; }
        public int PatientProfileId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string TimeSlot { get; set; } = string.Empty;
        public int QueueNumber { get; set; }      // Số thứ tự khám (STT)
        public decimal Fee { get; set; }
        public string? Note { get; set; }
        public string Status { get; set; } = "Pending";

        // Navigation
        public Order Order { get; set; } = null!;
        public Doctor Doctor { get; set; } = null!;
        public PatientProfile PatientProfile { get; set; } = null!;
    }
}
