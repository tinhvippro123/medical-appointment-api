namespace MedicalAppointment.API.Services.Interface;

public interface IEmailService
{
    bool IsSmtpConfigured { get; }
    Task<bool> SendOtpEmailAsync(string toEmail, string fullName, string otp, string purposeTitle);
}
