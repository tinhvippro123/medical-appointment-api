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

    // --- DOCTOR DAY-OFF ---
    public static readonly ErrorCode DAY_OFF_ALREADY_EXISTS = new("DAY_OFF_ALREADY_EXISTS", "Bác sĩ đã đăng ký nghỉ vào ngày này!");
    public static readonly ErrorCode DAY_OFF_IN_PAST = new("DAY_OFF_IN_PAST", "Không thể đăng ký ngày nghỉ trong quá khứ!");
    public static readonly ErrorCode DAY_OFF_NOT_FOUND = new("DAY_OFF_NOT_FOUND", "Không tìm thấy ngày nghỉ của bác sĩ!");
    public static ErrorCode DoctorDayOffHasOrders(int count) =>
        new("DOCTOR_DAY_OFF_HAS_ORDERS", $"Bác sĩ đã có {count} đơn đặt lịch vào ngày này, không thể đăng ký ngày nghỉ!");

    // --- PATIENT PROFILE ---
    public static readonly ErrorCode PROFILE_IN_USE = new("PROFILE_IN_USE", "Hồ sơ đang được sử dụng trong giỏ hàng hoặc đơn hàng, không thể xóa!");

    // --- CART ---
    public static readonly ErrorCode INVALID_PATIENT_PROFILE = new("INVALID_PATIENT_PROFILE", "Hồ sơ bệnh nhân không hợp lệ!");
    public static readonly ErrorCode CART_ITEM_DUPLICATE = new("CART_ITEM_DUPLICATE", "Lịch hẹn này đã có trong giỏ hàng!");
    public static readonly ErrorCode INVALID_TIME_SLOT = new("INVALID_TIME_SLOT", "Khung giờ không nằm trong lịch làm việc của bác sĩ!");

    // --- BOOKING ---
    public static readonly ErrorCode TIME_SLOT_FULL = new("TIME_SLOT_FULL", "Khung giờ này đã đầy, vui lòng chọn giờ khác!");

    // --- ORDER ---
    public static readonly ErrorCode ORDER_NOT_FOUND = new("ORDER_NOT_FOUND", "Không tìm thấy đơn đặt lịch!");
    public static readonly ErrorCode INVALID_ORDER_STATUS_TRANSITION = new("INVALID_ORDER_STATUS_TRANSITION", "Chuyển trạng thái đơn hàng không hợp lệ!");
    public static readonly ErrorCode INVALID_DATE_RANGE = new("INVALID_DATE_RANGE", "Ngày bắt đầu không được lớn hơn ngày kết thúc!");

    // --- CHECKOUT & ORDER ---
    public static readonly ErrorCode CART_EMPTY = new("CART_EMPTY", "Giỏ hàng của bạn đang trống!");
    public static readonly ErrorCode DOCTOR_ON_LEAVE = new("DOCTOR_ON_LEAVE", "Bác sĩ nghỉ khám vào ngày này!");
    public static readonly ErrorCode APPOINTMENT_TIME_PASSED = new("APPOINTMENT_TIME_PASSED", "Giờ khám đã trôi qua, vui lòng chọn khung giờ khác!");
    public static readonly ErrorCode PATIENT_SCHEDULE_CONFLICT = new("PATIENT_SCHEDULE_CONFLICT", "Bệnh nhân đã có lịch khám trong khung giờ này!");
    public static readonly ErrorCode CHECKOUT_CONFLICT = new("CHECKOUT_CONFLICT", "Giỏ hàng vừa thay đổi hoặc hệ thống đang bận, vui lòng thử lại!");
    public static readonly ErrorCode DOCTOR_UNAVAILABLE = new("DOCTOR_UNAVAILABLE", "Bác sĩ hiện không nhận lịch khám!");

    // --- GENERAL ---
    public static readonly ErrorCode RESOURCE_NOT_FOUND = new("RESOURCE_NOT_FOUND", "Không tìm thấy tài nguyên yêu cầu!");
    public static readonly ErrorCode UNAUTHORIZED = new("UNAUTHORIZED", "Vui lòng đăng nhập để tiếp tục!");
    public static readonly ErrorCode FORBIDDEN = new("FORBIDDEN", "Bạn không có quyền thực hiện hành động này!");
    public static readonly ErrorCode INTERNAL_SERVER_ERROR = new("INTERNAL_SERVER_ERROR", "Đã xảy ra lỗi hệ thống nghiêm trọng!");

    // --- USER & ROLE MANAGEMENT ---
    public static readonly ErrorCode EMAIL_ALREADY_EXISTS = new("EMAIL_ALREADY_EXISTS", "Email này đã được sử dụng!");
    public static readonly ErrorCode CANNOT_DISABLE_SELF = new("CANNOT_DISABLE_SELF", "Bạn không thể tự khóa hoặc xóa tài khoản của chính mình!");
    public static readonly ErrorCode CANNOT_CHANGE_OWN_ROLE = new("CANNOT_CHANGE_OWN_ROLE", "Bạn không thể tự thay đổi vai trò của chính mình!");
    public static readonly ErrorCode ROLE_NOT_FOUND = new("ROLE_NOT_FOUND", "Không tìm thấy vai trò!");
    public static readonly ErrorCode ROLE_KEY_ALREADY_EXISTS = new("ROLE_KEY_ALREADY_EXISTS", "Mã vai trò này đã tồn tại!");
    public static readonly ErrorCode ROLE_IN_USE = new("ROLE_IN_USE", "Vai trò này đang được gán cho người dùng, không thể xóa!");
    public static readonly ErrorCode SYSTEM_ROLE_PROTECTED = new("SYSTEM_ROLE_PROTECTED", "Không thể xóa hoặc thay đổi mã của vai trò mặc định!");
}