using System.ComponentModel.DataAnnotations;

namespace MedicalAppointment.API.DTOs.Cart;

public class AddCartItemRequest : IValidatableObject
{
    [Required(ErrorMessage = "Vui lòng chọn bác sĩ")]
    public int? DoctorId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn hồ sơ bệnh nhân")]
    public int? PatientProfileId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ngày khám")]
    public DateOnly? AppointmentDate { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn khung giờ khám")]
    [StringLength(20, ErrorMessage = "Khung giờ không được vượt quá 20 ký tự")]
    [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d-([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Định dạng khung giờ không hợp lệ (ví dụ: 08:00-09:00)")]
    public string TimeSlot { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
    public string? Note { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (AppointmentDate.HasValue)
        {
            var todayVn = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
            if (AppointmentDate.Value < todayVn)
            {
                yield return new ValidationResult(
                    "Ngày khám không được ở trong quá khứ",
                    [nameof(AppointmentDate)]
                );
            }
        }

        if (!string.IsNullOrWhiteSpace(TimeSlot))
        {
            var parts = TimeSlot.Trim().Split('-');
            if (parts.Length == 2 &&
                TimeSpan.TryParse(parts[0], out var startTime) &&
                TimeSpan.TryParse(parts[1], out var endTime))
            {
                if (startTime >= endTime)
                {
                    yield return new ValidationResult(
                        "Giờ bắt đầu phải nhỏ hơn giờ kết thúc",
                        [nameof(TimeSlot)]
                    );
                }
            }
        }
    }
}
