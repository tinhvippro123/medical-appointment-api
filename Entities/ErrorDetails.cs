using System.Text.Json;

namespace MedicalAppointment.API.Entities;

public class ErrorDetails
{
    public int StatusCode { get; set; }

    public string ErrorCode { get; set; }

    public string Message { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

}
