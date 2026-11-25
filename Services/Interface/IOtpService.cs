namespace MedicalAppointment.API.Services.Interface;

public interface IOtpService
{
    string GenerateOtp(string purpose, string identifier, TimeSpan ttl);
    bool VerifyAndConsumeOtp(string purpose, string identifier, string otp);
}
