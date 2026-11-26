using System.ComponentModel.DataAnnotations;

namespace MedicalAppointment.API.DTOs.PatientProfile;

public abstract class PatientProfileRequestBase : IValidatableObject
{
    [Required(ErrorMessage = "Họ tên không được để trống!")]
    [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự!")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ngày sinh không được để trống!")]
    public DateTime? DateOfBirth { get; set; }

    [Required(ErrorMessage = "Giới tính không được để trống!")]
    [StringLength(10, ErrorMessage = "Giới tính không được vượt quá 10 ký tự!")]
    public string Gender { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Dân tộc không được vượt quá 50 ký tự!")]
    public string? Ethnicity { get; set; }

    [StringLength(100, ErrorMessage = "Nghề nghiệp không được vượt quá 100 ký tự!")]
    public string? Occupation { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    [StringLength(20, ErrorMessage = "CCCD/CMND không được vượt quá 20 ký tự!")]
    public string? IdentityCardNumber { get; set; }

    [StringLength(50, ErrorMessage = "Số BHYT không được vượt quá 50 ký tự!")]
    public string? HealthInsuranceNumber { get; set; }

    [Required(ErrorMessage = "Mối quan hệ không được để trống!")]
    [StringLength(50, ErrorMessage = "Mối quan hệ không được vượt quá 50 ký tự!")]
    public string Relationship { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DateOfBirth.HasValue)
        {
            if (DateOfBirth.Value > DateTime.UtcNow)
            {
                yield return new ValidationResult("Ngày sinh không được ở tương lai!", [nameof(DateOfBirth)]);
            }

            if (DateOfBirth.Value < new DateTime(1900, 1, 1))
            {
                yield return new ValidationResult("Ngày sinh không được trước năm 1900!", [nameof(DateOfBirth)]);
            }
        }
    }
}
