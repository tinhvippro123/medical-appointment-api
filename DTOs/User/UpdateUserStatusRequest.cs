using System.ComponentModel.DataAnnotations;

namespace MedicalAppointment.API.DTOs.User;

public class UpdateUserStatusRequest
{
    public bool IsActive {get; set;}
    
}