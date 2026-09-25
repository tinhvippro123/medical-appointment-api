namespace MedicalAppointment.Shared.Entities
{
    public class Role : BaseEntity
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;        // "ADMIN", "PATIENT"
        public string Name { get; set; } = string.Empty;        // "Quản trị viên", "Bệnh nhân"
        public string? Description { get; set; }

        // Navigation
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
