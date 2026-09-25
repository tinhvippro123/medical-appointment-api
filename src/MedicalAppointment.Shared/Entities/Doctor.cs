namespace MedicalAppointment.Shared.Entities
{
    public class Doctor : BaseEntity
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Qualification { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int DepartmentId { get; set; }
        public int? RoomId { get; set; }
        public string? Description { get; set; }
        public string? AvatarUrl { get; set; }
        public int ExperienceYears { get; set; }
        public decimal ConsultationFee { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public Department Department { get; set; } = null!;
        public Room? Room { get; set; }
        public ICollection<DoctorSchedule> Schedules { get; set; } = new List<DoctorSchedule>();
        public ICollection<DoctorDayOff> DayOffs { get; set; } = new List<DoctorDayOff>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
