# Medical Appointment API

REST API Backend cho hệ thống đặt lịch khám bệnh (ASP.NET Core 10 + Entity Framework Core + SQL Server).

## Công nghệ sử dụng

- **Framework:** ASP.NET Core 10 Web API
- **Cơ sở dữ liệu:** Microsoft SQL Server (EF Core Code-First + script T-SQL thuần)
- **Xác thực:** JWT Bearer Token, BCrypt password hashing, xác thực OTP Email, đăng nhập Google / Facebook / Zalo

## Cấu trúc thư mục

```text
medical-appointment-api/
├── Controllers/     # API Controllers (Auth, Users, Roles, Departments, Rooms, Doctors, Orders, Payments, Cart, PatientProfiles)
├── Data/            # ApplicationDbContext và DbSeeder
├── Database/        # MedicalAppointmentDb.sql (script tạo DB, 11 bảng & dữ liệu mẫu cho SSMS)
├── DTOs/            # Request / Response models
├── Entities/        # 11 Entity tương ứng 11 bảng trong SQL Server
├── Exceptions/      # Custom exceptions & mã lỗi ErrorCode
├── Helpers/         # Lưu ảnh trong wwwroot/images
├── Middlewares/     # GlobalExceptionHandler
├── Migrations/      # EF Core Migrations
├── Provider/        # JwtProvider, PasswordHasher
├── Services/        # Business logic services
├── wwwroot/         # Ảnh chuyên khoa và bác sĩ
├── Program.cs       # Cấu hình pipeline, DI, JWT, Migration & Seeder
└── appsettings.json # Chuỗi kết nối SQL Server
```

## Cơ sở dữ liệu

Hệ thống gồm **11 bảng**: `Roles`, `Users`, `Departments`, `Rooms`, `Doctors`, `DoctorSchedules`, `DoctorDayOffs`, `PatientProfiles`, `CartItems`, `Orders`, `OrderDetails`.

Có thể khởi tạo theo 2 cách:
1. Tự động qua EF Core Migration & `DbSeeder` khi chạy `dotnet run`.
2. Mở file `Database/MedicalAppointmentDb.sql` bằng SQL Server Management Studio (SSMS) và nhấn **F5** để tạo database, ràng buộc và dữ liệu mẫu trực tiếp.

## Chạy dự án

```bash
dotnet restore
dotnet run
```

Mặc định API chạy ở `http://localhost:5041` (Swagger UI tại `/swagger` trong môi trường Development).
Tài khoản mẫu mặc định sau khi seed:
- **Admin:** `0987654321` / mật khẩu `123456`
- **Bệnh nhân:** `0912345678` / mật khẩu `123456`

## Các nhóm API chính

- **Auth (`/api/auth`):** `register`, `login`, `social-login`, `verify-email`, `forgot-password`, `reset-password`, `change-password`, `verify-password`, `me`, `logout`
- **Quản lý hệ thống (`/api/users`, `/api/roles`, `/api/admin/orders`):** quản lý người dùng, vai trò, đơn đặt lịch (Admin)
- **Danh mục & Lịch khám (`/api/departments`, `/api/rooms`, `/api/doctors`, `/api/doctors/{id}/day-offs`, `/api/doctors/{id}/schedules`):** tra cứu công khai và CRUD dành cho Admin
- **Đặt lịch & Thanh toán (`/api/patient-profiles`, `/api/cart`, `/api/orders`, `/api/payments`):** quản lý hồ sơ bệnh nhân, giỏ hàng, đặt lịch và thanh toán VNPay / VietQR

Tham khảo thêm: [Tài liệu Rooms API](docs/rooms-api.md)
