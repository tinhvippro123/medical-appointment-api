using System.ComponentModel.DataAnnotations;

namespace MedicalAppointment.API.DTOs.DoctorSchedule;

public abstract class DoctorScheduleRequestBase : IValidatableObject
{
    [Required(ErrorMessage = "Vui lòng chọn thứ trong tuần.")]
    [Range(0, 6, ErrorMessage = "DayOfWeek phải từ 0 đến 6 (Chủ nhật đến Thứ 7).")]
    public int? DayOfWeek { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập giờ bắt đầu.")]
    public TimeSpan? StartTime { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập giờ kết thúc.")]
    public TimeSpan? EndTime { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "MaxPatients phải lớn hơn 0.")]
    public int? MaxPatients { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartTime.HasValue)
        {
            if (StartTime.Value < TimeSpan.Zero || StartTime.Value >= TimeSpan.FromHours(24))
            {
                yield return new ValidationResult(
                    "StartTime phải lớn hơn hoặc bằng 00:00:00 và nhỏ hơn 24:00:00.",
                    new[] { nameof(StartTime) });
            }
        }

        if (EndTime.HasValue)
        {
            if (EndTime.Value <= TimeSpan.Zero || EndTime.Value >= TimeSpan.FromHours(24))
            {
                yield return new ValidationResult(
                    "EndTime phải lớn hơn 00:00:00 và nhỏ hơn 24:00:00.",
                    new[] { nameof(EndTime) });
            }
        }

        if (StartTime.HasValue && EndTime.HasValue)
        {
            if (StartTime.Value >= EndTime.Value)
            {
                yield return new ValidationResult(
                    "StartTime phải nhỏ hơn EndTime.",
                    new[] { nameof(StartTime), nameof(EndTime) });
            }
        }
    }
}
