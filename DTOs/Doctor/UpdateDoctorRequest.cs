namespace MedicalAppointment.API.DTOs.Doctor;

/// <summary>
/// DTO cho PUT /api/doctors/{id} (cập nhật bác sĩ).
/// PUT thay thế toàn bộ trường: trường optional không gửi thì thành null trong DB.
/// Riêng Image: không gửi thì giữ ảnh cũ, gửi thì thay thế ảnh.
/// Dùng [FromForm] vì có upload ảnh (multipart/form-data).
/// </summary>
public class UpdateDoctorRequest : DoctorRequestBase
{
}
