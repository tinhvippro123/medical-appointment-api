namespace MedicalAppointment.API.DTOs.PatientProfile;

public class UpdatePatientProfileRequest
{
    public required string FullName { get; set; }
    public required DateTime DateOfBirth { get; set; }
    public required string Gender { get; set; }
    public string? Ethnicity { get; set; }
    public string? Occupation { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? IdentityCardNumber { get; set; }
    public string? HealthInsuranceNumber { get; set; }
    public required string Relationship { get; set; }
}
