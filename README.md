# 🏥 Medical Appointment API

REST API Backend cho ứng dụng Đăng ký Khám Chữa Bệnh.

## 📚 Tài liệu

| Tài liệu | Mô tả |
|-----------|-------|
| [Kiến trúc dự án](docs/ARCHITECTURE.md) | Giải thích tại sao chọn Multi-Module, Clean Architecture |
| [Hướng dẫn Commit và PR](docs/CONTRIBUTING.md) | Quy trình commit, đặt tên branch, tạo Pull Request |

## 🛠️ Tech Stack

- **Framework:** ASP.NET Core 10
- **Database:** SQL Server
- **ORM:** Entity Framework Core
- **Authentication:** JWT Bearer Token
- **Architecture:** Multi-Module Monolith + Clean Architecture

## 📁 Cấu Trúc Project

```
MedicalAppointmentAPI.slnx
└ src/
    ├ MedicalAppointment.API/          → Entry point (Program.cs)
    ├ MedicalAppointment.Shared/       → Dùng chung (DbContext, Base)
    ├ MedicalAppointment.Auth/         → Module Xác thực
    ├ MedicalAppointment.Department/   → Module Chuyên khoa
    ├ MedicalAppointment.Doctor/       → Module Bác sĩ
    └ MedicalAppointment.Booking/      → Module Đặt lịch khám
```

## 👥 Nhóm phát triển

| Module | Thành viên | Phụ trách |
|--------|-----------|-----------|
| Shared + Auth | Tính | DbContext, Đăng ký, Đăng nhập, JWT, Role |
| Department | Triết | CRUD Chuyên khoa, Upload ảnh |
| Doctor | Thịnh | CRUD Bác sĩ, Tìm kiếm, Chi tiết |
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
- POST /api/auth/register — Đăng ký
- POST /api/auth/login — Đăng nhập (trả JWT token)
- POST /api/auth/logout — Đăng xuất
- PUT /api/auth/change-password — Đổi mật khẩu

### Department (/api/departments)
- GET /api/departments — Danh sách chuyên khoa
- POST /api/departments — Thêm (Admin)
- PUT /api/departments/{id} — Sửa (Admin)
- DELETE /api/departments/{id} — Xóa (Admin)

### Doctor (/api/doctors)
- GET /api/doctors — Danh sách bác sĩ (phân trang)
- GET /api/doctors/{id} — Chi tiết bác sĩ
- GET /api/doctors/department/{id} — Bác sĩ theo chuyên khoa
- GET /api/doctors/search?keyword= — Tìm kiếm
- POST /api/doctors — Thêm (Admin)

### Booking (/api/appointments)
- GET /api/appointments/cart — Xem giỏ hàng
- POST /api/appointments/cart — Thêm lịch hẹn vào giỏ
- PUT /api/appointments/cart/{id} — Cập nhật lịch hẹn
- DELETE /api/appointments/cart/{id} — Xóa khỏi giỏ
- POST /api/appointments/checkout — Xác nhận đặt lịch
- GET /api/appointments — Lịch sử đặt lịch

## 🌿 Git Workflow

```
main (production)
  └→ develop (tích hợp)
       ├→ feature/auth-login        Tính
       ├→ feature/department-crud   Triết
       ├→ feature/doctor-list       Thịnh
       └→ feature/cart-checkout     Thuận
```