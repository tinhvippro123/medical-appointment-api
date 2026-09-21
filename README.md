# Medical Appointment API

REST API Backend cho ung dung Dang ky Kham Chua Benh.

## Tai lieu

| Tai lieu | Mo ta |
|----------|-------|
| [Kien truc du an](docs/ARCHITECTURE.md) | Giai thich tai sao chon Multi-Module, Clean Architecture |
| [Huong dan Commit va PR](docs/CONTRIBUTING.md) | Quy trinh commit, dat ten branch, tao Pull Request |

## Tech Stack

- **Framework:** ASP.NET Core 10
- **Database:** SQL Server
- **ORM:** Entity Framework Core
- **Authentication:** JWT Bearer Token
- **Architecture:** Multi-Module Monolith + Clean Architecture

## Cau Truc Project

```
MedicalAppointmentAPI.slnx
  src/
    MedicalAppointment.API/          -> Entry point (Program.cs)
    MedicalAppointment.Shared/       -> Dung chung (DbContext, Base)
    MedicalAppointment.Auth/         -> Module Xac thuc
    MedicalAppointment.Department/   -> Module Chuyen khoa
    MedicalAppointment.Doctor/       -> Module Bac si
    MedicalAppointment.Booking/      -> Module Dat lich kham
```

## Team

| Module | Owner | Phu trach |
|--------|-------|-----------|
| Shared + Auth | Tinh | DbContext, Dang ky, Dang nhap, JWT, Role |
| Department | Triet | CRUD Chuyen khoa, Upload anh |
| Doctor | Thinh | CRUD Bac si, Tim kiem, Chi tiet |
| Booking | Thuan | Gio hang, Dat lich, Thanh toan |

## Chay project

```bash
git clone https://github.com/tinhvippro123/medical-appointment-api.git
cd medical-appointment-api
git checkout develop
dotnet restore
dotnet run --project src/MedicalAppointment.API
```

## API Endpoints

### Auth (/api/auth)
- POST /api/auth/register
- POST /api/auth/login
- POST /api/auth/logout
- PUT /api/auth/change-password

### Department (/api/departments)
- GET /api/departments
- POST /api/departments (Admin)
- PUT /api/departments/{id} (Admin)
- DELETE /api/departments/{id} (Admin)

### Doctor (/api/doctors)
- GET /api/doctors (phan trang)
- GET /api/doctors/{id}
- GET /api/doctors/department/{id}
- GET /api/doctors/search?keyword=
- POST /api/doctors (Admin)

### Booking (/api/appointments)
- GET /api/appointments/cart
- POST /api/appointments/cart
- PUT /api/appointments/cart/{id}
- DELETE /api/appointments/cart/{id}
- POST /api/appointments/checkout
- GET /api/appointments

## Git Workflow

```
main (production)
  -> develop (integration)
       -> feature/auth-login        Tinh
       -> feature/department-crud   Triet
       -> feature/doctor-list       Thinh
       -> feature/cart-checkout     Thuan
```
