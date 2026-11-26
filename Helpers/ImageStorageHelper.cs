using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace MedicalAppointment.API.Helpers;

public class ImageStorageHelper
{
    private readonly IWebHostEnvironment _env;
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly Regex SubfolderRegex = new(@"^[a-zA-Z0-9_-]+$", RegexOptions.Compiled);

    public ImageStorageHelper(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveImageAsync(IFormFile file, string subfolder)
    {
        if (file == null)
        {
            throw new ArgumentNullException(nameof(file));
        }

        if (string.IsNullOrWhiteSpace(subfolder) || !SubfolderRegex.IsMatch(subfolder))
        {
            throw new ArgumentException("Tên subfolder không hợp lệ. Chỉ chấp nhận chữ cái, chữ số, dấu gạch dưới và gạch ngang.", nameof(subfolder));
        }

        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
        {
            throw new ArgumentException("Định dạng file không được hỗ trợ. Chỉ chấp nhận .jpg, .jpeg, .png, .webp.", nameof(file));
        }

        var webRootPath = _env.WebRootPath;
        if (string.IsNullOrEmpty(webRootPath))
        {
            webRootPath = Path.Combine(_env.ContentRootPath, "wwwroot");
        }

        var folder = Path.Combine(webRootPath, "images", subfolder);
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(folder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/images/{subfolder}/{fileName}";
    }

    // Chỉ xóa khi đường dẫn trỏ vào wwwroot/images/, bỏ qua nếu không hợp lệ
    public void DeleteImage(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return;
        }

        try
        {
            var webRootPath = _env.WebRootPath;
            if (string.IsNullOrEmpty(webRootPath))
            {
                webRootPath = Path.Combine(_env.ContentRootPath, "wwwroot");
            }

            var imagesFolder = Path.GetFullPath(Path.Combine(webRootPath, "images"));
            if (!imagesFolder.EndsWith(Path.DirectorySeparatorChar))
            {
                imagesFolder += Path.DirectorySeparatorChar;
            }

            var normalizedPath = relativePath.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            var targetFilePath = Path.GetFullPath(Path.Combine(webRootPath, normalizedPath));

            if (!targetFilePath.StartsWith(imagesFolder, StringComparison.OrdinalIgnoreCase))
            {
                return; // Không nằm dưới wwwroot/images/ -> bỏ qua
            }

            if (File.Exists(targetFilePath))
            {
                File.Delete(targetFilePath);
            }
        }
        catch
        {
            // Bỏ qua lỗi xóa file vật lý để không làm gián đoạn luồng chính
        }
    }
}
