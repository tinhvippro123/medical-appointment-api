# Giải Thích Kiến Trúc Dự Án

## 1. Tại sao chọn Multi-Module Monolith?

### Multi-Module là gì?

Chia 1 ứng dụng thành nhiều MODULE riêng biệt, mỗi module phụ trách 1 chức năng cụ thể,
nhưng tất cả chạy CHUNG 1 app, CHUNG 1 database.

### So sánh kiến trúc

| Kiến trúc | Mô tả | Phù hợp |
|-----------|-------|---------|
| Monolith (1 cục) | Tất cả code trong 1 project | App nhỏ, 1-2 người |
| **Multi-Module** | Chia nhiều module, chạy chung 1 app | **App vừa, nhóm 3-5 người** |
| Microservices | Mỗi module là 1 app riêng, DB riêng | Hệ thống lớn, team 10+ người |

### Tại sao KHÔNG dùng Microservices?

- App đặt lịch khám bệnh chỉ cần 1 database chung
- 4 người làm, không cần tách thành nhiều service riêng
- Deploy 1 app đơn giản hơn deploy 5-6 services
- Debug dễ hơn, không cần trace qua nhiều services

### Tại sao KHÔNG dùng Layered Architecture đơn thuần?

- Layered Architecture (chỉ có 1 project với các folder Controller, Service, Repository)
  sẽ khiến 4 người làm chung các folder, dễ bị CONFLICT git
- Multi-Module: mỗi người làm trong MODULE riêng, ít conflict

---

## 2. Clean Architecture trong dự án

### Nguyên tắc chính

- Code được chia thành các LAYER (tầng)
- Layer BÊN TRONG không phụ thuộc layer BÊN NGOÀI
- Dependency chỉ đi từ ngoài vào trong

### Backend C# — Các layer trong mỗi module

```
Controllers/     → PRESENTATION layer (nhận request, trả response)
     |
     v
Services/        → APPLICATION layer (xử lý business logic)
Interfaces/      → (định nghĩa interface để gọi giữa các module)
     |
     v
Entities/        → DOMAIN layer (định nghĩa dữ liệu, không phụ thuộc gì)
DTOs/            → (Data Transfer Objects — chuyển đổi dữ liệu)
```

### Flutter — Các layer trong mỗi feature

```
presentation/    → UI layer (màn hình, widget)
  screens/         (các màn hình)
  widgets/         (widget tái sử dụng)
  providers/       (quản lý state)
     |
     v
domain/          → Business logic layer
  entities/        (định nghĩa dữ liệu thuần)
  usecases/        (các hành động: login, đặt lịch...)
  repositories/    (interface — chỉ định nghĩa)
     |
     v
data/            → Data layer (giao tiếp bên ngoài)
  models/          (JSON serialization)
  datasources/     (gọi API, đọc SQLite)
  repositories/    (implement interface từ domain)
```

### Tại sao dùng Interface?

- Module Booking cần lấy thông tin Doctor
- Nhưng Booking KHÔNG gọi trực tiếp DoctorService
- Mà gọi qua IDoctorService (interface)
- Lợi ích: LOOSE COUPLING — dễ thay đổi, dễ test, dễ bảo trì

---

## 3. Cấu trúc module

### Backend (.NET 10)

```
MedicalAppointment.API/          → Entry point (Program.cs)
MedicalAppointment.Shared/       → Dùng chung (DbContext, Base class)
MedicalAppointment.Auth/         → Tính: Xác thực, JWT, Role
MedicalAppointment.Department/   → Triết: CRUD Chuyên khoa
MedicalAppointment.Doctor/       → Thịnh: CRUD Bác sĩ, Tìm kiếm
MedicalAppointment.Booking/      → Thuận: Đặt lịch, Thanh toán
```

### Module giao tiếp như nào?

- Gọi nhau qua INTERFACE + DEPENDENCY INJECTION
- KHÔNG gọi qua HTTP API (đó là microservices)
- Tất cả chạy trong 1 process, 1 database