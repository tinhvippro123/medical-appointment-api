using MedicalAppointment.API.DTOs;

namespace MedicalAppointment.API.Services.Interface;

public interface IAuthService
{
    Task<RegisterResponse> Register(RegisterRequest request);
    Task<string> Login(LoginRequest request);
    Task ChangePassword(int userId, ChangePasswordRequest request);
    Task<string?> ForgotPassword(ForgotPasswordRequest request);
    Task ResetPassword(ResetPasswordRequest request);
    Task VerifyEmail(VerifyEmailRequest request);
    Task<string?> ResendVerificationOtp(ResendVerificationRequest request);
    Task VerifyCurrentPassword(int userId, string password);
    Task<MyProfileResponse> GetMyProfile(int userId);
    Task<UpdateMyProfileResponse> UpdateMyProfile(int userId, UpdateMyProfileRequest request);
}