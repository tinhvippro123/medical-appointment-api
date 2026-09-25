namespace MedicalAppointment.Shared.Entities
{
    public class Room : BaseEntity
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;   // "A101", "B205"
        public int Floor { get; set; }
        public int DepartmentId { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public Department Department { get; set; } = null!;
        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}
