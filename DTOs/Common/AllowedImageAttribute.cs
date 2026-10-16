using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MedicalAppointment.API.DTOs.Common;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class AllowedImageAttribute : ValidationAttribute
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"];
    private const long MaxFileSizeBytes = 2 * 1024 * 1024; // 2 MB

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not IFormFile file)
        {
            return ValidationResult.Success;
        }

        if (file.Length == 0)
        {
            return new ValidationResult("Tệp ảnh tải lên không được rỗng.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return new ValidationResult("Dung lượng ảnh đại diện không được vượt quá 2 MB.");
        }

        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
        {
            return new ValidationResult("Ảnh đại diện chỉ chấp nhận định dạng .jpg, .jpeg, .png, .webp.");
        }

        var contentType = file.ContentType?.ToLowerInvariant();
        if (string.IsNullOrEmpty(contentType) || !AllowedContentTypes.Contains(contentType))
        {
            return new ValidationResult("ContentType của ảnh không hợp lệ. Chỉ chấp nhận image/jpeg, image/png, image/webp.");
        }

        // Kiểm tra chữ ký file (magic bytes) từ OpenReadStream()
        try
        {
            using var stream = file.OpenReadStream();
            var header = new byte[12];
            var bytesRead = stream.Read(header, 0, header.Length);

            if (bytesRead < 12)
            {
                return new ValidationResult("Nội dung tệp ảnh không hợp lệ hoặc bị hỏng.");
            }

            // JPEG/JPG: FF D8 FF
            var isJpeg = header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;

            // PNG: 89 50 4E 47 0D 0A 1A 0A
            var isPng = header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47
                     && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A;

            // WebP: 0..3: 'RIFF' (0x52 0x49 0x46 0x46) và 8..11: 'WEBP' (0x57 0x45 0x42 0x50)
            var isWebp = header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46
                      && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50;

            if (extension is ".jpg" or ".jpeg")
            {
                if (!isJpeg)
                {
                    return new ValidationResult("Nội dung tệp không khớp với định dạng ảnh JPEG/JPG.");
                }
            }
            else if (extension == ".png")
            {
                if (!isPng)
                {
                    return new ValidationResult("Nội dung tệp không khớp với định dạng ảnh PNG.");
                }
            }
            else if (extension == ".webp")
            {
                if (!isWebp)
                {
                    return new ValidationResult("Nội dung tệp không khớp với định dạng ảnh WebP.");
                }
            }
        }
        catch
        {
            return new ValidationResult("Không thể đọc nội dung tệp ảnh tải lên.");
        }

        return ValidationResult.Success;
    }
}
