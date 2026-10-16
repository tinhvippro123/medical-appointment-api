namespace MedicalAppointment.API.Exceptions;

public class ErrorCode(string code, string message)
{
    public string Code { get; } = code;
    public string Message { get; } = message;

    public static readonly ErrorCode PHONE_ALREADY_EXISTS = new("PHONE_ALREADY_EXISTS", "Số điện thoại này đã được sử dụng!");
        public static readonly ErrorCode INVALID_CREDENTIALS = new("INVALID_CREDENTIALS", "Số điện thoại hoặc mật khẩu không chính xác!");
        public static readonly ErrorCode ACCOUNT_DISABLED = new("ACCOUNT_DISABLED", "Tài khoản của bạn đã bị khóa!");
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
        // --- DOCTOR CRUD (B1) ---
        public static readonly ErrorCode DOCTOR_HAS_ORDERS = new("DOCTOR_HAS_ORDERS", "Bác sĩ đã có trong đơn hàng, không thể xóa!");
        public static readonly ErrorCode DOCTOR_EMAIL_ALREADY_EXISTS = new("DOCTOR_EMAIL_ALREADY_EXISTS", "Email bác sĩ đã được sử dụng!");
        public static readonly ErrorCode DEPARTMENT_NOT_FOUND = new("DEPARTMENT_NOT_FOUND", "Không tìm thấy chuyên khoa!");
        public static readonly ErrorCode ROOM_NOT_FOUND = new("ROOM_NOT_FOUND", "Không tìm thấy phòng khám!");
        public static readonly ErrorCode ROOM_NOT_IN_DEPARTMENT = new("ROOM_NOT_IN_DEPARTMENT", "Phòng khám không thuộc chuyên khoa đã chọn!");
}