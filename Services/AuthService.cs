using System.Net.Http.Headers;
using System.Text.Json;
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
    IHostEnvironment hostEnvironment,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration) : IAuthService
{
    private const string ForgotPasswordOtpPurpose = "forgot-password";
    private const string VerifyEmailOtpPurpose = "verify-email";
    private static readonly TimeSpan OtpValidDuration = TimeSpan.FromMinutes(5);

    private sealed record VerifiedSocialIdentity(
        string ProviderUserId,
        string FullName,
        string? Email,
        bool IsEmailVerified,
        string? AvatarUrl);

    public async Task<RegisterResponse> Register(RegisterRequest request)
    {
        var phone = request.Phone.Trim();
        var normalizedEmail = string.IsNullOrWhiteSpace(request.Email)
            ? null
            : request.Email.Trim().ToLowerInvariant();

        var existingByPhone = await context.Users.FirstOrDefaultAsync(u => u.Phone == phone);
        if (existingByPhone != null)
        {
            // Chỉ cho phép đăng ký lại nếu tài khoản mới tạo đang chờ xác thực email (chưa từng cập nhật/khóa bởi Admin và chưa có dữ liệu)
            var canReusePending = normalizedEmail != null &&
                !existingByPhone.IsActive &&
                existingByPhone.Email != null &&
                existingByPhone.UpdatedAt == null &&
                !await context.Orders.AnyAsync(o => o.UserId == existingByPhone.Id) &&
                !await context.PatientProfiles.AnyAsync(p => p.UserId == existingByPhone.Id);

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

        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new NotFoundException(ErrorCode.USER_NOT_FOUND);
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

    public async Task VerifyCurrentPassword(int userId, string password)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new NotFoundException(ErrorCode.USER_NOT_FOUND);

        if (!passwordHasher.Verifty(password, user.Password))
        {
            throw new BadRequestException(ErrorCode.WRONG_OLD_PASSWORD);
        }
    }

    public async Task<MyProfileResponse> GetMyProfile(int userId)
    {
        var user = await context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new NotFoundException(ErrorCode.USER_NOT_FOUND);

        var selfProfile = await context.PatientProfiles
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.Relationship == "Bản thân")
            .ThenBy(p => p.Id)
            .FirstOrDefaultAsync();

        return new MyProfileResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Phone = user.Phone,
            Email = user.Email,
            RoleKey = user.Role?.Key ?? "PATIENT",
            RoleName = user.Role?.Name ?? "Bệnh nhân",
            DateOfBirth = selfProfile != null ? DateOnly.FromDateTime(selfProfile.DateOfBirth) : null,
            Gender = selfProfile?.Gender
        };
    }

    public async Task<UpdateMyProfileResponse> UpdateMyProfile(int userId, UpdateMyProfileRequest request)
    {
        var user = await context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new NotFoundException(ErrorCode.USER_NOT_FOUND);

        var normalizedEmail = string.IsNullOrWhiteSpace(request.Email)
            ? null
            : request.Email.Trim().ToLowerInvariant();

        if (normalizedEmail != null)
        {
            var emailTaken = await context.Users.AnyAsync(
                u => u.Id != userId && u.Email != null && u.Email.ToLower() == normalizedEmail);
            if (emailTaken)
            {
                throw new BadRequestException(ErrorCode.EMAIL_ALREADY_EXISTS);
            }
        }

        user.FullName = request.FullName.Trim();
        user.Email = normalizedEmail;

        var selfProfile = await context.PatientProfiles
            .Where(p => p.UserId == userId && p.Relationship == "Bản thân")
            .FirstOrDefaultAsync();

        if (selfProfile != null)
        {
            selfProfile.FullName = user.FullName;
            if (request.DateOfBirth.HasValue)
            {
                selfProfile.DateOfBirth = request.DateOfBirth.Value.ToDateTime(TimeOnly.MinValue);
            }
            if (!string.IsNullOrWhiteSpace(request.Gender))
            {
                selfProfile.Gender = request.Gender.Trim();
            }
        }
        else if (request.DateOfBirth.HasValue || !string.IsNullOrWhiteSpace(request.Gender))
        {
            context.PatientProfiles.Add(new PatientProfile
            {
                UserId = userId,
                FullName = user.FullName,
                DateOfBirth = request.DateOfBirth?.ToDateTime(TimeOnly.MinValue) ?? new DateTime(2000, 1, 1),
                Gender = string.IsNullOrWhiteSpace(request.Gender) ? "Khác" : request.Gender.Trim(),
                Phone = user.Phone,
                Relationship = "Bản thân"
            });
        }

        await context.SaveChangesAsync();

        var newToken = jwtProvider.GenerateToken(user);
        var updatedProfile = await GetMyProfile(userId);

        return new UpdateMyProfileResponse
        {
            Message = "Cập nhật thông tin thành công.",
            Token = newToken,
            Profile = updatedProfile
        };
    }

    public async Task<SocialLoginResponse> SocialLogin(SocialLoginRequest request)
    {
        var rawProvider = request.Provider.Trim().ToLowerInvariant();
        var (providerKey, displayProvider, phonePrefix) = rawProvider switch
        {
            "google" or "gmail" => ("google", "Google", "GG_"),
            "facebook" => ("facebook", "Facebook", "FB_"),
            "zalo" => ("zalo", "Zalo", "ZL_"),
            _ => throw new BadRequestException(ErrorCode.INVALID_SOCIAL_PROVIDER)
        };

        var verified = providerKey switch
        {
            "google" => await ResolveGoogleIdentityAsync(request),
            "facebook" => await ResolveFacebookIdentityAsync(request),
            "zalo" => await ResolveZaloIdentityAsync(request),
            _ => throw new BadRequestException(ErrorCode.INVALID_SOCIAL_PROVIDER)
        };

        var socialId = verified.ProviderUserId.Trim();
        var fullName = string.IsNullOrWhiteSpace(verified.FullName)
            ? $"Người dùng {displayProvider}"
            : verified.FullName.Trim();
        if (fullName.Length > 95)
        {
            fullName = fullName[..95];
        }

        var normalizedEmail = string.IsNullOrWhiteSpace(verified.Email)
            ? null
            : verified.Email.Trim().ToLowerInvariant();
        var avatarUrl = string.IsNullOrWhiteSpace(verified.AvatarUrl)
            ? null
            : verified.AvatarUrl.Trim();

        // 1. Luôn ưu tiên khớp chính xác bằng ProviderUserId đã được provider xác thực
        User? user = providerKey switch
        {
            "google" => await context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.GoogleId == socialId),
            "facebook" => await context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.FacebookId == socialId),
            "zalo" => await context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.ZaloId == socialId),
            _ => null
        };

        // 2. Chỉ cho phép tự liên kết theo Email khi provider xác nhận email_verified = true (VD: Google),
        //    KHÔNG bao giờ tự liên kết bằng Email/SĐT tự khai báo từ client và KHÔNG tự liên kết vào tài khoản ADMIN.
        if (user == null && normalizedEmail != null && verified.IsEmailVerified)
        {
            var existingByEmail = await context.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == normalizedEmail);

            if (existingByEmail != null)
            {
                if (string.Equals(existingByEmail.Role?.Key, "ADMIN", StringComparison.OrdinalIgnoreCase))
                {
                    throw new BadRequestException(ErrorCode.SocialAuthFailed(
                        "Tài khoản quản trị vui lòng đăng nhập bằng số điện thoại và mật khẩu."));
                }

                user = existingByEmail;
            }
        }

        var isNewUser = false;
        if (user != null)
        {
            if (!user.IsActive)
            {
                throw new BadRequestException(ErrorCode.ACCOUNT_DISABLED);
            }

            switch (providerKey)
            {
                case "google":
                    user.GoogleId ??= socialId;
                    break;
                case "facebook":
                    user.FacebookId ??= socialId;
                    break;
                case "zalo":
                    user.ZaloId ??= socialId;
                    break;
            }

            if (avatarUrl != null && string.IsNullOrWhiteSpace(user.AvatarUrl))
            {
                user.AvatarUrl = avatarUrl;
            }

            await context.SaveChangesAsync();
        }
        else
        {
            isNewUser = true;
            var patientRole = await context.Roles.FirstOrDefaultAsync(r => r.Key == "PATIENT")
                ?? throw new Exception("Role PATIENT không tồn tại trong DB.");

            // Nếu email chưa verify bị trùng với user khác trong DB thì không gán email để tránh lỗi unique index
            string? emailForNewUser = normalizedEmail;
            if (emailForNewUser != null &&
                await context.Users.AnyAsync(u => u.Email != null && u.Email.ToLower() == emailForNewUser))
            {
                emailForNewUser = null;
            }

            string assignedPhone;
            do
            {
                assignedPhone = $"{phonePrefix}{Random.Shared.Next(1000000, 9999999)}";
            } while (await context.Users.AnyAsync(u => u.Phone == assignedPhone));

            user = new User
            {
                FullName = fullName,
                Email = emailForNewUser,
                Phone = assignedPhone,
                Password = passwordHasher.Hash(Guid.NewGuid().ToString("N")),
                AvatarUrl = avatarUrl,
                RoleId = patientRole.Id,
                Role = patientRole,
                GoogleId = providerKey == "google" ? socialId : null,
                FacebookId = providerKey == "facebook" ? socialId : null,
                ZaloId = providerKey == "zalo" ? socialId : null,
                IsActive = true
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            context.PatientProfiles.Add(new PatientProfile
            {
                UserId = user.Id,
                FullName = fullName,
                DateOfBirth = new DateTime(2000, 1, 1),
                Gender = "Khác",
                Phone = null,
                Relationship = "Bản thân"
            });
            await context.SaveChangesAsync();
        }

        return new SocialLoginResponse
        {
            Token = jwtProvider.GenerateToken(user),
            Message = $"Đăng nhập bằng {displayProvider} thành công!",
            Provider = displayProvider,
            IsNewUser = isNewUser
        };
    }

    private async Task<VerifiedSocialIdentity> ResolveGoogleIdentityAsync(SocialLoginRequest request)
    {
        var http = httpClientFactory.CreateClient();

        try
        {
            if (!string.IsNullOrWhiteSpace(request.IdToken))
            {
                var resp = await http.GetAsync(
                    $"https://oauth2.googleapis.com/tokeninfo?id_token={Uri.EscapeDataString(request.IdToken.Trim())}");

                if (!resp.IsSuccessStatusCode)
                {
                    throw new BadRequestException(ErrorCode.SOCIAL_TOKEN_INVALID);
                }

                using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
                var root = doc.RootElement;
                var sub = GetJsonString(root, "sub");
                if (string.IsNullOrWhiteSpace(sub))
                {
                    throw new BadRequestException(ErrorCode.SOCIAL_TOKEN_INVALID);
                }

                var aud = GetJsonString(root, "aud");
                var expectedWebClientId = configuration["SocialAuth:Google:WebClientId"];
                var expectedAndroidClientId = configuration["SocialAuth:Google:AndroidClientId"];
                if (!string.IsNullOrWhiteSpace(aud)
                    && !string.IsNullOrWhiteSpace(expectedWebClientId)
                    && !string.Equals(aud, expectedWebClientId, StringComparison.Ordinal)
                    && !string.Equals(aud, expectedAndroidClientId, StringComparison.Ordinal))
                {
                    throw new BadRequestException(ErrorCode.SOCIAL_TOKEN_INVALID);
                }

                // Chỉ tin email do Google trả về (không lấy request.Email từ client)
                var email = GetJsonString(root, "email");
                var isEmailVerified = IsJsonTrue(root, "email_verified");
                var name = GetJsonString(root, "name") ?? email ?? "Người dùng Google";
                var picture = GetJsonString(root, "picture");

                return new VerifiedSocialIdentity(sub, name, email, isEmailVerified, picture);
            }

            if (!string.IsNullOrWhiteSpace(request.AccessToken))
            {
                using var msg = new HttpRequestMessage(
                    HttpMethod.Get,
                    "https://www.googleapis.com/oauth2/v3/userinfo");
                msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", request.AccessToken.Trim());

                var resp = await http.SendAsync(msg);
                if (!resp.IsSuccessStatusCode)
                {
                    throw new BadRequestException(ErrorCode.SOCIAL_TOKEN_INVALID);
                }

                using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
                var root = doc.RootElement;
                var sub = GetJsonString(root, "sub");
                if (string.IsNullOrWhiteSpace(sub))
                {
                    throw new BadRequestException(ErrorCode.SOCIAL_TOKEN_INVALID);
                }

                var email = GetJsonString(root, "email");
                var isEmailVerified = IsJsonTrue(root, "email_verified");
                var name = GetJsonString(root, "name") ?? email ?? "Người dùng Google";
                var picture = GetJsonString(root, "picture");

                return new VerifiedSocialIdentity(sub, name, email, isEmailVerified, picture);
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException)
        {
            throw new BadRequestException(ErrorCode.SOCIAL_TOKEN_INVALID);
        }

        return FallbackDevIdentity(request);
    }

    private async Task<VerifiedSocialIdentity> ResolveFacebookIdentityAsync(SocialLoginRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.AccessToken))
        {
            try
            {
                var http = httpClientFactory.CreateClient();
                var token = request.AccessToken.Trim();

                // Xác minh access token thuộc đúng Facebook AppId của hệ thống (nếu đã cấu hình)
                var expectedAppId = configuration["SocialAuth:Facebook:AppId"]?.Trim();
                if (!string.IsNullOrWhiteSpace(expectedAppId))
                {
                    var appResp = await http.GetAsync(
                        $"https://graph.facebook.com/app?fields=id&access_token={Uri.EscapeDataString(token)}");
                    if (!appResp.IsSuccessStatusCode)
                    {
                        throw new BadRequestException(ErrorCode.SOCIAL_TOKEN_INVALID);
                    }

                    using var appDoc = JsonDocument.Parse(await appResp.Content.ReadAsStringAsync());
                    var tokenAppId = GetJsonString(appDoc.RootElement, "id");
                    if (!string.Equals(tokenAppId, expectedAppId, StringComparison.Ordinal))
                    {
                        throw new BadRequestException(ErrorCode.SOCIAL_TOKEN_INVALID);
                    }
                }

                var url = $"https://graph.facebook.com/me?fields=id,name,email,picture.type(large)&access_token={Uri.EscapeDataString(token)}";
                var resp = await http.GetAsync(url);

                if (!resp.IsSuccessStatusCode)
                {
                    throw new BadRequestException(ErrorCode.SOCIAL_TOKEN_INVALID);
                }

                using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
                var root = doc.RootElement;
                var fbId = GetJsonString(root, "id");
                if (string.IsNullOrWhiteSpace(fbId))
                {
                    throw new BadRequestException(ErrorCode.SOCIAL_TOKEN_INVALID);
                }

                var name = GetJsonString(root, "name") ?? "Người dùng Facebook";
                var email = GetJsonString(root, "email");
                string? avatar = null;
                if (root.TryGetProperty("picture", out var picObj)
                    && picObj.TryGetProperty("data", out var dataObj))
                {
                    avatar = GetJsonString(dataObj, "url");
                }

                // Không tự link tài khoản có sẵn bằng email từ Facebook (chỉ khớp qua FacebookId)
                return new VerifiedSocialIdentity(fbId, name, email, false, avatar);
            }
            catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException)
            {
                throw new BadRequestException(ErrorCode.SOCIAL_TOKEN_INVALID);
            }
        }

        return FallbackDevIdentity(request);
    }

    private async Task<VerifiedSocialIdentity> ResolveZaloIdentityAsync(SocialLoginRequest request)
    {
        var http = httpClientFactory.CreateClient();
        string? zaloAccessToken = request.AccessToken?.Trim();

        try
        {
            if (!string.IsNullOrWhiteSpace(request.OAuthCode))
            {
                var appId = configuration["SocialAuth:Zalo:AppId"]?.Trim();
                var appSecret = configuration["SocialAuth:Zalo:AppSecret"]?.Trim();
                if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(appSecret))
                {
                    throw new BadRequestException(
                        ErrorCode.SocialAuthFailed("Chưa cấu hình Zalo AppId hoặc AppSecret trên máy chủ!"));
                }

                var formFields = new Dictionary<string, string>
                {
                    ["code"] = request.OAuthCode.Trim(),
                    ["app_id"] = appId,
                    ["grant_type"] = "authorization_code"
                };
                if (!string.IsNullOrWhiteSpace(request.CodeVerifier))
                {
                    formFields["code_verifier"] = request.CodeVerifier.Trim();
                }

                using var tokenReq = new HttpRequestMessage(
                    HttpMethod.Post,
                    "https://oauth.zaloapp.com/v4/access_token")
                {
                    Content = new FormUrlEncodedContent(formFields)
                };
                tokenReq.Headers.Add("secret_key", appSecret);

                var tokenResp = await http.SendAsync(tokenReq);
                var tokenBody = await tokenResp.Content.ReadAsStringAsync();
                if (!tokenResp.IsSuccessStatusCode && string.IsNullOrWhiteSpace(tokenBody))
                {
                    throw new BadRequestException(ErrorCode.SOCIAL_TOKEN_INVALID);
                }

                using var tokenDoc = JsonDocument.Parse(tokenBody);
                var tokenRoot = tokenDoc.RootElement;

                zaloAccessToken = GetJsonString(tokenRoot, "access_token");
                if (string.IsNullOrWhiteSpace(zaloAccessToken))
                {
                    var errDesc = GetJsonString(tokenRoot, "error_description")
                        ?? GetJsonString(tokenRoot, "error_name")
                        ?? "Không đổi được mã xác thực Zalo.";
                    throw new BadRequestException(ErrorCode.SocialAuthFailed($"Zalo OAuth: {errDesc}"));
                }
            }

            if (!string.IsNullOrWhiteSpace(zaloAccessToken))
            {
                using var meReq = new HttpRequestMessage(
                    HttpMethod.Get,
                    "https://graph.zalo.me/v2.0/me?fields=id,name,picture");
                meReq.Headers.Add("access_token", zaloAccessToken);

                var meResp = await http.SendAsync(meReq);
                if (!meResp.IsSuccessStatusCode)
                {
                    throw new BadRequestException(ErrorCode.SOCIAL_TOKEN_INVALID);
                }

                using var meDoc = JsonDocument.Parse(await meResp.Content.ReadAsStringAsync());
                var meRoot = meDoc.RootElement;
                var zaloId = GetJsonString(meRoot, "id");
                if (string.IsNullOrWhiteSpace(zaloId))
                {
                    var msg = GetJsonString(meRoot, "message") ?? "Không đọc được thông tin tài khoản Zalo.";
                    throw new BadRequestException(ErrorCode.SocialAuthFailed(msg));
                }

                var name = GetJsonString(meRoot, "name") ?? "Người dùng Zalo";
                string? avatar = null;
                if (meRoot.TryGetProperty("picture", out var picObj)
                    && picObj.TryGetProperty("data", out var dataObj))
                {
                    avatar = GetJsonString(dataObj, "url");
                }

                // Zalo không cung cấp email/SĐT đã xác thực — tuyệt đối không tin dữ liệu client tự khai báo
                return new VerifiedSocialIdentity(zaloId, name, null, false, avatar);
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException)
        {
            throw new BadRequestException(ErrorCode.SOCIAL_TOKEN_INVALID);
        }

        return FallbackDevIdentity(request);
    }

    private VerifiedSocialIdentity FallbackDevIdentity(SocialLoginRequest request)
    {
        if (!hostEnvironment.IsDevelopment() || string.IsNullOrWhiteSpace(request.ProviderUserId))
        {
            throw new BadRequestException(ErrorCode.SOCIAL_TOKEN_INVALID);
        }

        return new VerifiedSocialIdentity(
            request.ProviderUserId.Trim(),
            string.IsNullOrWhiteSpace(request.FullName) ? "Người dùng" : request.FullName.Trim(),
            request.Email,
            false,
            request.AvatarUrl);
    }

    private static bool IsJsonTrue(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var prop))
        {
            return false;
        }

        return prop.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.String => string.Equals(prop.GetString(), "true", StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    private static string? GetJsonString(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop))
        {
            return prop.ValueKind switch
            {
                JsonValueKind.String => prop.GetString(),
                JsonValueKind.Number => prop.GetRawText(),
                _ => null
            };
        }
        return null;
    }
}
