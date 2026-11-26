using System.Text.Json;

namespace MedicalAppointment.API.Entities;

public class ErrorDetails
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    public int StatusCode { get; set; }

    public string ErrorCode { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonOptions);
    }

}
