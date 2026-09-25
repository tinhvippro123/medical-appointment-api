namespace MedicalAppointment.Shared.Entities
{
    public class CartItem : BaseEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int DoctorId { get; set; }
        public int PatientProfileId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string TimeSlot { get; set; } = string.Empty;  // "07:00-08:00"
        public string? Note { get; set; }

        // Navigation
        public User User { get; set; } = null!;
        public Doctor Doctor { get; set; } = null!;
        public PatientProfile PatientProfile { get; set; } = null!;
    }
}
