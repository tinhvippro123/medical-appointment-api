using System.ComponentModel.DataAnnotations;

namespace MedicalAppointment.API.DTOs.User;

public class UpdateUserRoleRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn vai trò hợp lệ")]
    public int RoleId{get; set;}
}