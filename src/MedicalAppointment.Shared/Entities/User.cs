namespace MedicalAppointment.Shared.Entities
{
    public class User : BaseEntity
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public int RoleId { get; set; }

        // Social Login (Bonus)
        public string? GoogleId { get; set; }
        public string? FacebookId { get; set; }
        public string? ZaloId { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation
        public Role Role { get; set; } = null!;
        public ICollection<PatientProfile> PatientProfiles { get; set; } = new List<PatientProfile>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
