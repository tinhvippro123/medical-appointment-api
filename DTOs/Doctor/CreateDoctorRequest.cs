namespace MedicalAppointment.API.DTOs.Doctor;

/// <summary>
/// DTO cho POST /api/doctors (tạo mới bác sĩ).
/// Dùng [FromForm] vì có upload ảnh (multipart/form-data).
/// </summary>
public class CreateDoctorRequest : DoctorRequestBase
{
}
