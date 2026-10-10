namespace MedicalAppointment.API.Exceptions;

public class ErrorCode(string code, string message)
{
    public string Code { get; } = code;
    public string Message { get; } = message;

    public static readonly ErrorCode PHONE_ALREADY_EXISTS = new("PHONE_ALREADY_EXISTS", "Số điện thoại này đã được sử dụng!");
        public static readonly ErrorCode INVALID_CREDENTIALS = new("INVALID_CREDENTIALS", "Số điện thoại hoặc mật khẩu không chính xác!");
        public static readonly ErrorCode ACCOUNT_DISABLED = new("ACCOUNT_DISABLED", "Tài khoản của bạn đã bị khóa!");
        // --- DEPARTMENT ---
        public static readonly ErrorCode DEPARTMENT_NOT_FOUND = new("DEPARTMENT_NOT_FOUND", "Không tìm thấy chuyên khoa!");
        public static readonly ErrorCode DEPARTMENT_NAME_EXISTS = new("DEPARTMENT_NAME_EXISTS", "Tên chuyên khoa đã tồn tại!");
        public static readonly ErrorCode DEPARTMENT_HAS_DOCTORS = new("DEPARTMENT_HAS_DOCTORS", "Khoa còn bác sĩ, không thể xóa!");
        public static readonly ErrorCode DEPARTMENT_IMAGE_INVALID = new("DEPARTMENT_IMAGE_INVALID", "File ảnh không hợp lệ! Chỉ chấp nhận .jpg, .jpeg, .png và dung lượng dưới 5MB.");
        public static readonly ErrorCode DEPARTMENT_NO_IMAGE = new("DEPARTMENT_NO_IMAGE", "Chuyên khoa này chưa có ảnh!");
        // --- DOCTOR ---
        public static readonly ErrorCode DOCTOR_NOT_FOUND = new("DOCTOR_NOT_FOUND", "Không tìm thấy thông tin bác sĩ!");
        public static readonly ErrorCode DOCTOR_SCHEDULE_CONFLICT = new("DOCTOR_SCHEDULE_CONFLICT", "Bác sĩ đã có lịch trong khung giờ này!");
        // --- BOOKING ---
        public static readonly ErrorCode TIME_SLOT_FULL = new("TIME_SLOT_FULL", "Khung giờ này đã đầy, vui lòng chọn giờ khác!");
        // --- GENERAL ---
        public static readonly ErrorCode RESOURCE_NOT_FOUND = new("RESOURCE_NOT_FOUND", "Không tìm thấy tài nguyên yêu cầu!");
        public static readonly ErrorCode UNAUTHORIZED = new("UNAUTHORIZED", "Vui lòng đăng nhập để tiếp tục!");
        public static readonly ErrorCode FORBIDDEN = new("FORBIDDEN", "Bạn không có quyền thực hiện hành động này!");
        public static readonly ErrorCode INTERNAL_SERVER_ERROR = new("INTERNAL_SERVER_ERROR", "Đã xảy ra lỗi hệ thống nghiêm trọng!");
}