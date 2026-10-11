using MedicalAppointment.API.DTOs;

namespace MedicalAppointment.API.Services.Interface;

public interface IAuthService
{
    Task Register(RegisterRequest request);
    Task<string> Login(LoginRequest request);
}