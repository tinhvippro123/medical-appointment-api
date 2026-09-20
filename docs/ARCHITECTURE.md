# Giai Thich Kien Truc Du An

## 1. Tai sao chon Multi-Module Monolith?

### Multi-Module la gi?

Chia 1 ung dung thanh nhieu MODULE rieng biet, moi module phu trach 1 chuc nang cu the,
nhung tat ca chay CHUNG 1 app, CHUNG 1 database.

### So sanh kien truc

| Kien truc | Mo ta | Phu hop |
|-----------|-------|---------|
| Monolith (1 cuc) | Tat ca code trong 1 project | App nho, 1-2 nguoi |
| **Multi-Module** | Chia nhieu module, chay chung 1 app | **App vua, nhom 3-5 nguoi** |
| Microservices | Moi module la 1 app rieng, DB rieng | He thong lon, team 10+ nguoi |

### Tai sao KHONG dung Microservices?

- App dat lich kham benh chi can 1 database chung
- 4 nguoi lam, khong can tach thanh nhieu service rieng
- Deploy 1 app don gian hon deploy 5-6 services
- Debug de hon, khong can trace qua nhieu services

### Tai sao KHONG dung Layered Architecture don thuan?

- Layered Architecture (chi co 1 project voi cac folder Controller, Service, Repository)
  se khien 4 nguoi lam chung cac folder, de bi CONFLICT git
- Multi-Module: moi nguoi lam trong MODULE rieng, it conflict

---

## 2. Clean Architecture trong du an

### Nguyen tac chinh

- Code duoc chia thanh cac LAYER (tang)
- Layer BEN TRONG khong phu thuoc layer BEN NGOAI
- Dependency chi di tu ngoai vao trong

### Backend C# - Cac layer trong moi module

```
Controllers/     -> PRESENTATION layer (nhan request, tra response)
     |
     v
Services/        -> APPLICATION layer (xu ly business logic)
Interfaces/      -> (dinh nghia interface de goi giua cac module)
     |
     v
Entities/        -> DOMAIN layer (dinh nghia du lieu, khong phu thuoc gi)
DTOs/            -> (Data Transfer Objects - chuyen doi du lieu)
```

### Flutter - Cac layer trong moi feature

```
presentation/    -> UI layer (man hinh, widget)
  screens/         (cac man hinh)
  widgets/         (widget tai su dung)
  providers/       (quan ly state)
     |
     v
domain/          -> Business logic layer
  entities/        (dinh nghia du lieu thuan)
  usecases/        (cac hanh dong: login, dat lich...)
  repositories/    (interface - chi dinh nghia)
     |
     v
data/            -> Data layer (giao tiep ben ngoai)
  models/          (JSON serialization)
  datasources/     (goi API, doc SQLite)
  repositories/    (implement interface tu domain)
```

### Tai sao dung Interface?

- Module Booking can lay thong tin Doctor
- Nhung Booking KHONG goi truc tiep DoctorService
- Ma goi qua IDoctorService (interface)
- Loi ich: LOOSE COUPLING - de thay doi, de test, de bao tri

---

## 3. Cau truc module

### Backend (.NET 10)

```
MedicalAppointment.API/          -> Entry point (Program.cs)
MedicalAppointment.Shared/       -> Dung chung (DbContext, Base class)
MedicalAppointment.Auth/         -> Tinh: Xac thuc, JWT, Role
MedicalAppointment.Department/   -> Triet: CRUD Chuyen khoa
MedicalAppointment.Doctor/       -> Thinh: CRUD Bac si, Tim kiem
MedicalAppointment.Booking/      -> Thuan: Dat lich, Thanh toan
```

### Module giao tiep nhu nao?

- Goi nhau qua INTERFACE + DEPENDENCY INJECTION
- KHONG goi qua HTTP API (do la microservices)
- Tat ca chay trong 1 process, 1 database
