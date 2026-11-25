using MedicalAppointment.API.Data;
using MedicalAppointment.API.DTOs;
using MedicalAppointment.API.Entities;
using MedicalAppointment.API.Exceptions;
using MedicalAppointment.API.Provider;
using MedicalAppointment.API.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace MedicalAppointment.API.Services;

public class AuthService(
    ApplicationDbContext context,
    IJwtProvider jwtProvider,
    IPasswordHasher passwordHasher,
    IOtpService otpService,
    IEmailService emailService,
    IHostEnvironment hostEnvironment) : IAuthService
{
    private const string ForgotPasswordOtpPurpose = "forgot-password";
    private const string VerifyEmailOtpPurpose = "verify-email";
    private static readonly TimeSpan OtpValidDuration = TimeSpan.FromMinutes(5);

    public async Task<RegisterResponse> Register(RegisterRequest request)
    {
        var phone = request.Phone.Trim();
        var normalizedEmail = string.IsNullOrWhiteSpace(request.Email)
            ? null
            : request.Email.Trim().ToLowerInvariant();

        var existingByPhone = await context.Users.FirstOrDefaultAsync(u => u.Phone == phone);
        if (existingByPhone != null)
        {
            var canReusePending = normalizedEmail != null &&
                !existingByPhone.IsActive &&
                !await context.Orders.AnyAsync(o => o.UserId == existingByPhone.Id);

            if (!canReusePending)
            {
                throw new BadRequestException(ErrorCode.PHONE_ALREADY_EXISTS);
            }

            var emailTakenByOther = await context.Users.AnyAsync(
                u => u.Id != existingByPhone.Id && u.Email != null && u.Email.ToLower() == normalizedEmail);
            if (emailTakenByOther)
            {
                throw new BadRequestException(ErrorCode.EMAIL_ALREADY_EXISTS);
            }

            existingByPhone.FullName = request.FullName.Trim();
            existingByPhone.Email = normalizedEmail;
            existingByPhone.Password = passwordHasher.Hash(request.Password);
            await context.SaveChangesAsync();

            var retryOtp = otpService.GenerateOtp(VerifyEmailOtpPurpose, normalizedEmail!, OtpValidDuration);
            var retrySent = await emailService.SendOtpEmailAsync(
                normalizedEmail!,
                existingByPhone.FullName,
                retryOtp,
                "xác thực tài khoản");

            return new RegisterResponse
            {
                Message = "Đã gửi lại mã OTP xác thực tới email của bạn. Vui lòng kiểm tra hộp thư để kích hoạt tài khoản.",
                RequiresEmailVerification = true,
                Email = normalizedEmail,
                DevOtp = !retrySent && hostEnvironment.IsDevelopment() ? retryOtp : null
            };
        }

        if (normalizedEmail != null)
        {
            var emailExists = await context.Users.AnyAsync(
                u => u.Email != null && u.Email.ToLower() == normalizedEmail);
            if (emailExists)
            {
                throw new BadRequestException(ErrorCode.EMAIL_ALREADY_EXISTS);
            }
        }

        var patientRole = await context.Roles.FirstOrDefaultAsync(r => r.Key == "PATIENT")
            ?? throw new Exception("Role PATIENT không tồn tại trong DB.");

        var requiresVerification = normalizedEmail != null;

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Phone = phone,
            Email = normalizedEmail,
            Password = passwordHasher.Hash(request.Password),
            RoleId = patientRole.Id,
            IsActive = !requiresVerification
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        if (!requiresVerification)
        {
            return new RegisterResponse
            {
                Message = "Đăng ký thành công.",
                RequiresEmailVerification = false
            };
        }

        var otp = otpService.GenerateOtp(VerifyEmailOtpPurpose, normalizedEmail!, OtpValidDuration);
        var sentViaSmtp = await emailService.SendOtpEmailAsync(
            normalizedEmail!,
            user.FullName,
            otp,
            "xác thực tài khoản");

        return new RegisterResponse
        {
            Message = "Đăng ký thành công! Vui lòng nhập mã OTP gửi tới email để kích hoạt tài khoản.",
            RequiresEmailVerification = true,
            Email = normalizedEmail,
            DevOtp = !sentViaSmtp && hostEnvironment.IsDevelopment() ? otp : null
        };
    }
    public async Task<string> Login(LoginRequest request)
    {
        var user = await context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Phone == request.Phone);
        if (user == null || !passwordHasher.Verifty(request.Password, user.Password))
        {
            throw new BadRequestException(ErrorCode.INVALID_CREDENTIALS);
        }

        if (!user.IsActive)
        {
            throw new BadRequestException(ErrorCode.ACCOUNT_DISABLED);
        }

        return jwtProvider.GenerateToken(user);
    }

    public async Task ChangePassword(int userId, ChangePasswordRequest request)
    {
        if (request.NewPassword != request.ConfirmPassword)
        {
            throw new BadRequestException(ErrorCode.PASSWORD_MISMATCH);
        }

        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId) ?? throw new BadRequestException(ErrorCode.USER_NOT_FOUND);
        if (!passwordHasher.Verifty(request.OldPassword, user.Password))
        {
            throw new BadRequestException(ErrorCode.WRONG_OLD_PASSWORD);
        }

        if (request.NewPassword == request.OldPassword)
        {
            throw new BadRequestException(ErrorCode.SAME_OLD_PASSWORD);
        }

        user.Password = passwordHasher.Hash(request.NewPassword);
        await context.SaveChangesAsync();
    }

    public async Task<string?> ForgotPassword(ForgotPasswordRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await context.Users.FirstOrDefaultAsync(
            u => u.Email != null && u.Email.ToLower() == normalizedEmail);

        if (user == null)
        {
            throw new BadRequestException(ErrorCode.EMAIL_NOT_FOUND);
        }

        if (!user.IsActive)
        {
            throw new BadRequestException(ErrorCode.ACCOUNT_DISABLED);
        }

        var otp = otpService.GenerateOtp(ForgotPasswordOtpPurpose, normalizedEmail, OtpValidDuration);
        var sentViaSmtp = await emailService.SendOtpEmailAsync(
            user.Email!,
            user.FullName,
            otp,
            "đặt lại mật khẩu");

        return !sentViaSmtp && hostEnvironment.IsDevelopment() ? otp : null;
    }

    public async Task ResetPassword(ResetPasswordRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await context.Users.FirstOrDefaultAsync(
            u => u.Email != null && u.Email.ToLower() == normalizedEmail);

        if (user == null)
        {
            throw new BadRequestException(ErrorCode.EMAIL_NOT_FOUND);
        }

        if (!user.IsActive)
        {
            throw new BadRequestException(ErrorCode.ACCOUNT_DISABLED);
        }

        var isOtpValid = otpService.VerifyAndConsumeOtp(
            ForgotPasswordOtpPurpose,
            normalizedEmail,
            request.Otp);

        if (!isOtpValid)
        {
            throw new BadRequestException(ErrorCode.INVALID_OR_EXPIRED_OTP);
        }

        user.Password = passwordHasher.Hash(request.NewPassword);
        await context.SaveChangesAsync();
    }

    public async Task VerifyEmail(VerifyEmailRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await context.Users.FirstOrDefaultAsync(
            u => u.Email != null && u.Email.ToLower() == normalizedEmail);

        if (user == null)
        {
            throw new BadRequestException(ErrorCode.EMAIL_NOT_FOUND);
        }

        if (user.IsActive)
        {
            return;
        }

        var isOtpValid = otpService.VerifyAndConsumeOtp(
            VerifyEmailOtpPurpose,
            normalizedEmail,
            request.Otp);

        if (!isOtpValid)
        {
            throw new BadRequestException(ErrorCode.INVALID_OR_EXPIRED_OTP);
        }

        user.IsActive = true;
        await context.SaveChangesAsync();
    }

    public async Task<string?> ResendVerificationOtp(ResendVerificationRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await context.Users.FirstOrDefaultAsync(
            u => u.Email != null && u.Email.ToLower() == normalizedEmail);

        if (user == null)
        {
            throw new BadRequestException(ErrorCode.EMAIL_NOT_FOUND);
        }

        if (user.IsActive)
        {
            throw new BadRequestException(ErrorCode.EMAIL_ALREADY_VERIFIED);
        }

        var otp = otpService.GenerateOtp(VerifyEmailOtpPurpose, normalizedEmail, OtpValidDuration);
        var sentViaSmtp = await emailService.SendOtpEmailAsync(
            user.Email!,
            user.FullName,
            otp,
            "xác thực tài khoản");

        return !sentViaSmtp && hostEnvironment.IsDevelopment() ? otp : null;
    }
}
