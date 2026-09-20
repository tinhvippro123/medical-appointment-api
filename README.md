# 🏥 Medical Appointment API

REST API Backend cho ứng dụng Đăng ký Khám Chữa Bệnh.

## 🛠️ Tech Stack

- **Framework:** ASP.NET Core 10
- **Database:** SQL Server
- **ORM:** Entity Framework Core
- **Authentication:** JWT Bearer Token
- **Architecture:** Multi-Module Monolith

## 📁 Cấu Trúc Project

```
MedicalAppointmentAPI.slnx
└── src/
    ├── MedicalAppointment.API/          → Entry point (Program.cs, DI config)
    ├── MedicalAppointment.Shared/       → Dùng chung (DbContext, Base, Helpers)
    ├── MedicalAppointment.Auth/         → 👤 Module Xác thực & Người dùng
    ├── MedicalAppointment.Department/   → 🏢 Module Chuyên khoa
    ├── MedicalAppointment.Doctor/       → 👨‍⚕️ Module Bác sĩ
    └── MedicalAppointment.Booking/      → 📅 Module Đặt lịch khám
```

## 🔗 Module Dependencies

```
API ──→ Auth, Department, Doctor, Booking
Booking ──→ Shared, Auth, Doctor
Doctor ──→ Shared, Department
Department ──→ Shared
Auth ──→ Shared
```

## 👥 Team Ownership

| Module | Owner | Mô tả |
|--------|-------|--------|
| Shared | Team Lead (A) | DbContext, Base entities, Helpers |
| Auth | Thành viên A | Đăng ký, Đăng nhập, JWT, Role, Phân quyền |
| Department | Thành viên B | CRUD Chuyên khoa, Upload ảnh |
| Doctor | Thành viên C | CRUD Bác sĩ, Lịch làm việc, Tìm kiếm |
| Booking | Thành viên D | Đặt lịch, Giỏ hàng, Thanh toán, Quản lý đơn |

## 🚀 Getting Started

### Yêu cầu
- .NET 10 SDK
- SQL Server (LocalDB hoặc Express)

### Chạy project
```bash
git clone <repository-url>
cd medical-appointment-api
dotnet restore
dotnet ef database update --project src/MedicalAppointment.Shared --startup-project src/MedicalAppointment.API
dotnet run --project src/MedicalAppointment.API
```

API sẽ chạy tại: http://localhost:5000

## 📋 API Endpoints

### Auth (/api/auth)
- POST /api/auth/register - Đăng ký
- POST /api/auth/login - Đăng nhập
- POST /api/auth/logout - Đăng xuất
- PUT /api/auth/change-password - Đổi mật khẩu

### Department (/api/departments)
- GET /api/departments - Danh sách chuyên khoa
- POST /api/departments - Thêm (Admin)
- PUT /api/departments/{id} - Sửa (Admin)
- DELETE /api/departments/{id} - Xóa (Admin)

### Doctor (/api/doctors)
- GET /api/doctors - Danh sách bác sĩ (phân trang)
- GET /api/doctors/{id} - Chi tiết bác sĩ
- GET /api/doctors/search?keyword= - Tìm kiếm
- POST /api/doctors - Thêm (Admin)

### Appointment (/api/appointments)
- GET /api/appointments/cart - Xem giỏ hàng
- POST /api/appointments/cart - Thêm lịch hẹn vào giỏ
- POST /api/appointments/checkout - Xác nhận đặt lịch
- GET /api/appointments - Lịch sử đặt lịch

## 🌿 Git Workflow

```
main (production)
  └── develop (integration)
       ├── feature/auth          ← Thành viên A
       ├── feature/department    ← Thành viên B
       ├── feature/doctor        ← Thành viên C
       └── feature/booking       ← Thành viên D
```
