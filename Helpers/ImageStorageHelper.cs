using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace MedicalAppointment.API.Helpers;

/// <summary>
/// Helper dùng chung để lưu và xóa ảnh trong wwwroot/images/{subfolder}/.
/// Nhận tên thư mục con (ví dụ "doctors", "departments") để dùng cho nhiều chức năng.
/// </summary>
public class ImageStorageHelper
{
    private readonly IWebHostEnvironment _env;
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly Regex SubfolderRegex = new(@"^[a-zA-Z0-9_-]+$", RegexOptions.Compiled);

    public ImageStorageHelper(IWebHostEnvironment env)
    {
        _env = env;
    }

    /// <summary>
    /// Lưu file ảnh vào wwwroot/images/{subfolder}/ với tên Guid mới.
    /// Trả về đường dẫn tương đối (ví dụ "/images/doctors/abc123.jpg").
    /// </summary>
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

    /// <summary>
    /// Xóa file ảnh từ đường dẫn tương đối (ví dụ "/images/doctors/abc123.jpg").
    /// Chỉ xóa khi file nằm dưới wwwroot/images/.
    /// Không ném lỗi nếu file không tồn tại hoặc đường dẫn không hợp lệ.
    /// </summary>
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
