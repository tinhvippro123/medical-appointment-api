namespace MedicalAppointment.API.Exceptions;

public class ErrorCode(string code, string message)
{
    public string Code { get; } = code;
    public string Message { get; } = message;

    // --- AUTH / USER ---
    public static readonly ErrorCode PHONE_ALREADY_EXISTS = new("PHONE_ALREADY_EXISTS", "Số điện thoại này đã được sử dụng!");
    public static readonly ErrorCode INVALID_CREDENTIALS = new("INVALID_CREDENTIALS", "Số điện thoại hoặc mật khẩu không chính xác!");
    public static readonly ErrorCode ACCOUNT_DISABLED = new("ACCOUNT_DISABLED", "Tài khoản của bạn đã bị khóa!");
    public static readonly ErrorCode USER_NOT_FOUND = new("USER_NOT_FOUND", "Không tìm thấy người dùng!");
    public static readonly ErrorCode PASSWORD_MISMATCH = new("PASSWORD_MISMATCH", "Mật khẩu xác nhận không khớp!");
    public static readonly ErrorCode WRONG_OLD_PASSWORD = new("WRONG_OLD_PASSWORD", "Mật khẩu cũ không chính xác!");
    public static readonly ErrorCode SAME_OLD_PASSWORD = new("SAME_OLD_PASSWORD", "Mật khẩu mới không được trùng với mật khẩu cũ!");

    // --- DEPARTMENT ---
    public static readonly ErrorCode DEPARTMENT_NOT_FOUND = new("DEPARTMENT_NOT_FOUND", "Không tìm thấy chuyên khoa!");
    public static readonly ErrorCode DEPARTMENT_NAME_EXISTS = new("DEPARTMENT_NAME_EXISTS", "Tên chuyên khoa đã tồn tại!");
    public static readonly ErrorCode DEPARTMENT_HAS_DOCTORS = new("DEPARTMENT_HAS_DOCTORS", "Khoa còn bác sĩ, không thể xóa!");
    public static readonly ErrorCode DEPARTMENT_IMAGE_INVALID = new("DEPARTMENT_IMAGE_INVALID", "File ảnh không hợp lệ! Chỉ chấp nhận ảnh JPG, JPEG, PNG không rỗng và tối đa 5MB.");
    public static readonly ErrorCode DEPARTMENT_IMAGE_URL_INVALID = new("DEPARTMENT_IMAGE_URL_INVALID", "URL ảnh phải là đường dẫn HTTP hoặc HTTPS hợp lệ.");
    public static readonly ErrorCode DEPARTMENT_NO_IMAGE = new("DEPARTMENT_NO_IMAGE", "Chuyên khoa này chưa có ảnh!");

    // --- DOCTOR ---
    public static readonly ErrorCode DOCTOR_NOT_FOUND = new("DOCTOR_NOT_FOUND", "Không tìm thấy thông tin bác sĩ!");
    public static readonly ErrorCode DOCTOR_SCHEDULE_CONFLICT = new("DOCTOR_SCHEDULE_CONFLICT", "Bác sĩ đã có lịch trong khung giờ này!");
    public static readonly ErrorCode DOCTOR_HAS_ORDERS = new("DOCTOR_HAS_ORDERS", "Bác sĩ đã có trong đơn hàng, không thể xóa!");
    public static readonly ErrorCode DOCTOR_EMAIL_ALREADY_EXISTS = new("DOCTOR_EMAIL_ALREADY_EXISTS", "Email bác sĩ đã được sử dụng!");
    public static readonly ErrorCode ROOM_NOT_FOUND = new("ROOM_NOT_FOUND", "Không tìm thấy phòng khám!");
    public static readonly ErrorCode ROOM_NOT_IN_DEPARTMENT = new("ROOM_NOT_IN_DEPARTMENT", "Phòng khám không thuộc chuyên khoa đã chọn!");

    // --- PATIENT PROFILE ---
    public static readonly ErrorCode PROFILE_IN_USE = new("PROFILE_IN_USE", "Hồ sơ đang được sử dụng trong giỏ hàng hoặc đơn hàng, không thể xóa!");

    // --- BOOKING ---
    public static readonly ErrorCode TIME_SLOT_FULL = new("TIME_SLOT_FULL", "Khung giờ này đã đầy, vui lòng chọn giờ khác!");

    // --- GENERAL ---
    public static readonly ErrorCode RESOURCE_NOT_FOUND = new("RESOURCE_NOT_FOUND", "Không tìm thấy tài nguyên yêu cầu!");
    public static readonly ErrorCode UNAUTHORIZED = new("UNAUTHORIZED", "Vui lòng đăng nhập để tiếp tục!");
    public static readonly ErrorCode FORBIDDEN = new("FORBIDDEN", "Bạn không có quyền thực hiện hành động này!");
    public static readonly ErrorCode INTERNAL_SERVER_ERROR = new("INTERNAL_SERVER_ERROR", "Đã xảy ra lỗi hệ thống nghiêm trọng!");
}