using System.Text.Json.Serialization;

namespace MedicalAppointment.API.DTOs;

public class RegisterResponse
{
    public string Message { get; set; } = string.Empty;
    public bool RequiresEmailVerification { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Email { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DevOtp { get; set; }
}
