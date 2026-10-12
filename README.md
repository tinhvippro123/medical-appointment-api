# 🏥 Medical Appointment API

REST API Backend cho Ứng dụng Đăng Ký Khám Chữa Bệnh.

## 📚 Tài liệu

| Tài liệu | Mô tả |
|-----------|-------|
| [Hướng dẫn Commit và PR](docs/CONTRIBUTING.md) | Quy trình commit, đặt tên branch, tạo Pull Request |

## 🔧 Tech Stack

- **Framework:** ASP.NET Core 10
- **Database:** SQL Server
- **ORM:** Entity Framework Core
- **Authentication:** JWT Bearer Token
- **Architecture:** Single Project (Monolith)

## 📁 Cấu Trúc Project

```
MedicalAppointmentAPI.slnx
└── src/
    └── MedicalAppointment.API/
        ├── Controllers/       👈 API Controllers
        ├── Data/              👈 DbContext + Migrations
        ├── DTOs/              👈 Request/Response models
        ├── Entities/          👈 Database Entities (12 bảng)
        ├── Helpers/           👈 JWT, Password Hashing
        ├── Middlewares/       👈 Custom Middleware
        ├── Services/          👈 Business Logic
        ├── Program.cs         👈 Entry point
        └── appsettings.json   👈 Cấu hình
```

## 👥 Nhóm phát triển

| Feature | Thành viên | Phụ trách |
|---------|-----------|-----------|
| Auth + Setup | Tính | Đăng ký, Đăng nhập, JWT, Role, User Management |
| Department + Room | Triệt | CRUD Chuyên khoa, Phòng khám, Upload ảnh |
| Doctor + Schedule | Thịnh | CRUD Bác sĩ, Lịch làm việc, Tìm kiếm |
| Booking | Thuận | Giỏ hàng, Đặt lịch, Thanh toán |

## 🚀 Chạy project

```bash
git clone https://github.com/tinhvippro123/medical-appointment-api.git
cd medical-appointment-api
git checkout develop
dotnet restore
dotnet run --project src/MedicalAppointment.API
```

## 📋 API Endpoints

### Auth (/api/auth)
- POST /api/auth/register → Đăng ký (SĐT + Mật khẩu)
- POST /api/auth/login → Đăng nhập (SĐT + Mật khẩu, trả JWT token)
- POST /api/auth/logout → Đăng xuất
- PUT /api/auth/change-password → Đổi mật khẩu

### Department (/api/departments)
- GET /api/departments → Danh sách chuyên khoa
- POST /api/departments → Thêm (Admin)
- PUT /api/departments/{id} → Sửa (Admin)
- DELETE /api/departments/{id} → Xóa (Admin)

### Doctor (/api/doctors)
- GET /api/doctors → Danh sách bác sĩ (phân trang)
- GET /api/doctors/{id} → Chi tiết bác sĩ
- GET /api/doctors/department/{id} → Bác sĩ theo chuyên khoa
- GET /api/doctors/search?keyword= → Tìm kiếm
- POST /api/doctors → Thêm (Admin)

### Booking (/api/appointments)
- GET /api/appointments/cart → Xem giỏ hàng
- POST /api/appointments/cart → Thêm lịch hẹn vào giỏ
- DELETE /api/appointments/cart/{id} → Xóa khỏi giỏ
- POST /api/appointments/checkout → Xác nhận đặt lịch
- GET /api/appointments → Lịch sử đặt lịch

## 🌿 Git Workflow

```
main (production)
  └── develop (tích hợp)
       ├── feature/auth-login        Tính
       ├── feature/department-crud   Triệt
       ├── feature/doctor-list       Thịnh
       └── feature/cart-checkout     Thuận
```