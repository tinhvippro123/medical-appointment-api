using System.ComponentModel.DataAnnotations;

namespace MedicalAppointment.API.DTOs.Room;

// Dùng chung cho thêm và sửa phòng.
public class RoomRequest
{
    [Required(ErrorMessage = "Số phòng không được để trống.")]
    [StringLength(20, ErrorMessage = "Số phòng tối đa 20 ký tự.")]
    public string RoomNumber { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Tầng phải lớn hơn 0.")]
    public int Floor { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Mã chuyên khoa phải lớn hơn 0.")]
    public int DepartmentId { get; set; }
}
