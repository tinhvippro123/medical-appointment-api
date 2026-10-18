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
        // --- AUTH / USER ---
        public static readonly ErrorCode USER_NOT_FOUND = new("USER_NOT_FOUND", "Không tìm thấy người dùng!");
        public static readonly ErrorCode PASSWORD_MISMATCH = new("PASSWORD_MISMATCH", "Mật khẩu xác nhận không khớp!");
        public static readonly ErrorCode WRONG_OLD_PASSWORD = new("WRONG_OLD_PASSWORD", "Mật khẩu cũ không chính xác!");
        public static readonly ErrorCode SAME_OLD_PASSWORD = new("SAME_OLD_PASSWORD", "Mật khẩu mới không được trùng với mật khẩu cũ!");

        // --- CART ---
        public static readonly ErrorCode INVALID_PATIENT_PROFILE = new("INVALID_PATIENT_PROFILE", "Hồ sơ bệnh nhân không hợp lệ!");
        public static readonly ErrorCode CART_ITEM_DUPLICATE = new("CART_ITEM_DUPLICATE", "Lịch hẹn này đã có trong giỏ hàng!");
        public static readonly ErrorCode INVALID_TIME_SLOT = new("INVALID_TIME_SLOT", "Khung giờ không nằm trong lịch làm việc của bác sĩ!");

}