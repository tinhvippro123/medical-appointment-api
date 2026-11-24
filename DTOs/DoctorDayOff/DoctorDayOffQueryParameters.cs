using System.ComponentModel.DataAnnotations;

namespace MedicalAppointment.API.DTOs.DoctorDayOff;

public class DoctorDayOffQueryParameters : IValidatableObject
{
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (From.HasValue && To.HasValue && From.Value > To.Value)
        {
            yield return new ValidationResult(
                "Ngày bắt đầu (from) phải nhỏ hơn hoặc bằng ngày kết thúc (to).",
                [nameof(From), nameof(To)]
            );
        }
    }
}
