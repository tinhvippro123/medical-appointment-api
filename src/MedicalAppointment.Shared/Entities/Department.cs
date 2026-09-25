namespace MedicalAppointment.Shared.Entities
{
    public class Department : BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}
