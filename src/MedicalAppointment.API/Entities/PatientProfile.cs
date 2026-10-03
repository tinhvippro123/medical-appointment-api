namespace MedicalAppointment.API.Entities
{
    public class PatientProfile : BaseEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;    // "Nam", "N?"
        public string? Ethnicity { get; set; }                // "Kinh", "Tày"...
        public string? Occupation { get; set; }               // Ngh? nghi?p
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? IdentityCardNumber { get; set; }       // CCCD / CMND
        public string? HealthInsuranceNumber { get; set; }    // BHYT
        public string Relationship { get; set; } = string.Empty; // "B?n thân", "Con", "Cha", "M?"

        // Navigation
        public User User { get; set; } = null!;
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}

