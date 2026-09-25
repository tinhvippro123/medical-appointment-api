namespace MedicalAppointment.Shared.Entities
{
    public class DoctorDayOff : BaseEntity
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public DateTime Date { get; set; }
        public string? Reason { get; set; }

        // Navigation
        public Doctor Doctor { get; set; } = null!;
    }
}
