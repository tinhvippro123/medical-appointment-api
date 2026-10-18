using System.ComponentModel.DataAnnotations;
using MedicalAppointment.API.DTOs.Common;

namespace MedicalAppointment.API.DTOs.Doctor;

/// <summary>
/// Lớp cơ sở cho Create và Update Doctor request.
/// PUT thay thế toàn bộ trường: trường optional không gửi thì thành null trong DB.
/// Riêng ảnh (Image): không gửi thì giữ ảnh cũ (chỉ áp dụng cho PUT).
/// </summary>
public abstract class DoctorRequestBase
{
    [Required(ErrorMessage = "Vui lòng nhập họ và tên bác sĩ")]
    [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự")]
    public string FullName { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Bằng cấp không được vượt quá 100 ký tự")]
    public string? Qualification { get; set; }

    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
    public string? Email { get; set; }

    /// <summary>
    /// Auth (RegisterRequest) chưa có regex phone, nên dùng quy tắc ^0\d{9}$ (10 chữ số, bắt đầu bằng 0).
    /// Đây là quy tắc ngoài Trello, cần xác nhận với nhóm.
    /// </summary>
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

    [Required(ErrorMessage = "Vui lòng nhập phí khám")]
    [Range(typeof(decimal), "0.01", "9999999999999999.99", ErrorMessage = "Phí khám phải lớn hơn 0 và không vượt quá giới hạn")]
    public decimal? ConsultationFee { get; set; }

    [AllowedImage]
    public IFormFile? Image { get; set; }
}
