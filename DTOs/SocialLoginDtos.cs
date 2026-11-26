using System.ComponentModel.DataAnnotations;

namespace MedicalAppointment.API.DTOs;

public class SocialLoginRequest
{
    [Required(ErrorMessage = "Vui lòng chọn phương thức đăng nhập (Google, Facebook, Zalo)")]
    [StringLength(20)]
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Google ID Token trả về từ Google Sign-In SDK
    /// </summary>
    public string? IdToken { get; set; }

    /// <summary>
    /// Access Token trả về từ Google Sign-In hoặc Facebook Login SDK
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// OAuth Authorization Code trả về từ Zalo SDK (OAuth v4)
    /// </summary>
    public string? OAuthCode { get; set; }

    /// <summary>
    /// PKCE Code Verifier đi kèm OAuthCode của Zalo OAuth v4
    /// </summary>
    public string? CodeVerifier { get; set; }

    /// <summary>
    /// Dự phòng cho môi trường kiểm thử tự động / Development
    /// </summary>
    [StringLength(128)]
    public string? ProviderUserId { get; set; }

    [StringLength(80)]
    public string? FullName { get; set; }

    [StringLength(150)]
    [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
    public string? Email { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(500)]
    public string? AvatarUrl { get; set; }
}

public class SocialLoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public bool IsNewUser { get; set; }
}
