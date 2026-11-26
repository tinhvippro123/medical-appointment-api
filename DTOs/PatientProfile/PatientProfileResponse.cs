namespace MedicalAppointment.API.DTOs.PatientProfile;

public class PatientProfileResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string? Ethnicity { get; set; }
    public string? Occupation { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? IdentityCardNumber { get; set; }
    public string? HealthInsuranceNumber { get; set; }
    public string Relationship { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
