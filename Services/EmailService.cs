using System.Net;
using System.Net.Mail;
using System.Text;
using MedicalAppointment.API.Exceptions;
using MedicalAppointment.API.Services.Interface;

namespace MedicalAppointment.API.Services;

public class EmailService(IConfiguration configuration, ILogger<EmailService> logger) : IEmailService
{
    public bool IsSmtpConfigured =>
        !string.IsNullOrWhiteSpace(configuration["EmailSettings:SmtpHost"]) &&
        !string.IsNullOrWhiteSpace(configuration["EmailSettings:Username"]) &&
        !string.IsNullOrWhiteSpace(configuration["EmailSettings:Password"]);

    public async Task<bool> SendOtpEmailAsync(string toEmail, string fullName, string otp, string purposeTitle)
    {
        if (!IsSmtpConfigured)
        {
            logger.LogInformation(
                "[OTP EMAIL FALLBACK] {Purpose} | Email: {Email} | Mã OTP: {Otp} (hiệu lực 5 phút)",
                purposeTitle,
                toEmail,
                otp);
            return false;
        }

        var host = configuration["EmailSettings:SmtpHost"]!;
        var port = int.TryParse(configuration["EmailSettings:SmtpPort"], out var parsedPort) ? parsedPort : 587;
        var enableSsl = !bool.TryParse(configuration["EmailSettings:EnableSsl"], out var ssl) || ssl;
        var username = configuration["EmailSettings:Username"]!.Trim();
        var password = configuration["EmailSettings:Password"]!.Replace(" ", "").Trim();
        var senderEmail = string.IsNullOrWhiteSpace(configuration["EmailSettings:SenderEmail"])
            ? username
            : configuration["EmailSettings:SenderEmail"]!;
        var senderName = string.IsNullOrWhiteSpace(configuration["EmailSettings:SenderName"])
            ? "Trung tâm Y tế Hiệp Phước"
            : configuration["EmailSettings:SenderName"]!;

        var htmlBody = $"""
            <div style="font-family: Arial, sans-serif; max-width: 520px; margin: 0 auto; padding: 24px; border: 1px solid #e2e8f0; border-radius: 12px;">
              <h2 style="color: #1d4ed8; margin-top: 0;">Trung tâm Y tế Hiệp Phước</h2>
              <p>Xin chào <strong>{WebUtility.HtmlEncode(fullName)}</strong>,</p>
              <p>Mã xác thực OTP cho yêu cầu <strong>{WebUtility.HtmlEncode(purposeTitle)}</strong> của bạn là:</p>
              <div style="font-size: 28px; font-weight: bold; letter-spacing: 6px; color: #1d4ed8; background: #eff6ff; padding: 14px 20px; text-align: center; border-radius: 10px; margin: 16px 0;">
                {WebUtility.HtmlEncode(otp)}
              </div>
              <p style="color: #64748b; font-size: 13px;">Mã có hiệu lực trong vòng <strong>5 phút</strong>. Tuyệt đối không chia sẻ mã này cho người khác.</p>
            </div>
            """;

        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName, Encoding.UTF8),
                Subject = $"[Y tế Hiệp Phước] Mã OTP {purposeTitle}",
                SubjectEncoding = Encoding.UTF8,
                Body = htmlBody,
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = true
            };
            message.To.Add(new MailAddress(toEmail));

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(username, password),
                Timeout = 15000
            };

            await client.SendMailAsync(message);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Không thể gửi email OTP tới {Email}", toEmail);
            throw new BadRequestException(ErrorCode.EMAIL_SEND_FAILED);
        }
    }
}
