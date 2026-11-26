using System.ComponentModel.DataAnnotations;
using MedicalAppointment.API.DTOs.Common;

namespace MedicalAppointment.API.DTOs.Doctor;

public abstract class DoctorRequestBase : IValidatableObject
{
    [Required(ErrorMessage = "Vui lòng nhập họ và tên bác sĩ")]
    [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự")]
    public string FullName { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Bằng cấp không được vượt quá 100 ký tự")]
    public string? Qualification { get; set; }

    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
    public string? Email { get; set; }

    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải gồm 10 chữ số và bắt đầu bằng số 0")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn chuyên khoa")]
    public int? DepartmentId { get; set; }

    public int? RoomId { get; set; }

    [StringLength(2000, ErrorMessage = "Mô tả không được vượt quá 2000 ký tự")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập số năm kinh nghiệm")]
    [Range(0, 70, ErrorMessage = "Số năm kinh nghiệm phải từ 0 đến 70")]
    public int? ExperienceYears { get; set; }

    [AllowedImage]
    public IFormFile? Image { get; set; }

    public string? AvatarUrl { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(AvatarUrl)) yield break;

        if (Image != null)
        {
            yield return new ValidationResult("Chỉ chọn một nguồn ảnh.", [nameof(Image), nameof(AvatarUrl)]);
        }

        if (!Uri.TryCreate(AvatarUrl.Trim(), UriKind.Absolute, out var uri)
            || (uri.Scheme != "http" && uri.Scheme != "https")
            || string.IsNullOrEmpty(uri.Host) || !string.IsNullOrEmpty(uri.UserInfo))
        {
            yield return new ValidationResult("URL ảnh phải là đường dẫn HTTP hoặc HTTPS hợp lệ.", [nameof(AvatarUrl)]);
        }
    }
}
