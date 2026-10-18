namespace MedicalAppointment.API.Entities;

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
    public ICollection<DoctorSchedule> Schedules { get; set; } = [];
    public ICollection<DoctorDayOff> DayOffs { get; set; } = [];
    public ICollection<CartItem> CartItems { get; set; } = [];
    public ICollection<OrderDetail> OrderDetails { get; set; } = [];
}

