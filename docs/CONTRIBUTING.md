# Hướng Dẫn Commit và Pull Request

## 1. Quy trình làm việc hàng ngày

```
Bước 1: Pull code mới nhất
  git checkout develop
  git pull origin develop

Bước 2: Tạo branch mới từ develop
  git checkout -b feature/tên-chức-năng

Bước 3: Code xong, commit
  git add .
  git commit -m "feat: mô tả ngắn gọn"

Bước 4: Push lên GitHub
  git push origin feature/tên-chức-năng

Bước 5: Vào GitHub tạo Pull Request
  Base: develop  <--  Compare: feature/tên-chức-năng

Bước 6: Đợi team review, approve rồi MERGE
```

---

## 2. Đặt tên Branch

```
feature/auth-login          ← Chức năng mới
feature/department-crud     ← Chức năng mới
fix/login-token-expired     ← Sửa lỗi
refactor/doctor-service     ← Tái cấu trúc code
```

### Ví dụ cụ thể cho từng người

```
Tính:   feature/auth-register
        feature/auth-login
        feature/auth-jwt
        feature/user-management
        feature/role-permission

Triết:  feature/department-crud
        feature/department-image-upload
        feature/doctor-crud-admin

Thịnh:  feature/home-screen
        feature/doctor-list
        feature/doctor-detail
        feature/doctor-search

Thuận:  feature/cart-add-remove
        feature/cart-update
        feature/checkout
        feature/order-history
        feature/payment-vnpay
```

---

## 3. Viết Commit Message

### Format

```
<loại>: <mô tả ngắn gọn bằng tiếng Việt hoặc tiếng Anh>
```

### Các loại commit

| Loại | Khi nào dùng | Ví dụ |
|------|-------------|-------|
| feat | Thêm chức năng mới | feat: thêm API đăng ký người dùng |
| fix | Sửa lỗi | fix: sửa lỗi token hết hạn khi login |
| refactor | Sửa code nhưng không đổi chức năng | refactor: tối ưu DoctorService |
| docs | Cập nhật tài liệu | docs: cập nhật README |
| style | Format code, sửa lỗi chính tả | style: format code AuthController |
| chore | Công việc setup, config | chore: thêm package Dio vào pubspec |
| test | Thêm/sửa test | test: thêm unit test cho AuthService |

### Ví dụ commit tốt

```
feat: thêm API POST /api/auth/register
feat: thêm màn hình đăng nhập
fix: sửa lỗi crash khi danh sách bác sĩ rỗng
refactor: tách BookingService thành các method nhỏ
docs: thêm API endpoints vào README
chore: cấu hình JWT trong appsettings.json
```

### Ví dụ commit XẤU (không nên)

```
update code              ← Quá chung chung
fix bug                  ← Bug gì? Ở đâu?
asdkjahsd               ← Vô nghĩa
thêm nhiều thứ           ← Thêm cái gì?
```

---

## 4. Tạo Pull Request trên GitHub

### Bước 1: Vào GitHub repo, click "Pull Requests" → "New Pull Request"

### Bước 2: Chọn branch

```
base: develop  <----  compare: feature/auth-login
```

### Bước 3: Điền thông tin PR

**Tiêu đề PR:**
```
[Tính] feat: Thêm chức năng đăng nhập và đăng ký
```

**Nội dung PR (copy mẫu này):**
```
## Mô tả
- Thêm API POST /api/auth/register (đăng ký)
- Thêm API POST /api/auth/login (đăng nhập, trả JWT token)
- Thêm màn hình Login và Register trên Flutter

## Checklist
- [ ] Code chạy được, không bị lỗi
- [ ] Đã test bằng Swagger/Postman
- [ ] Không conflict với branch develop

## Screenshot (nếu có UI)
(Dán hình màn hình ở đây)

## Ghi chú
- Cần merge PR của Tính (setup DbContext) trước khi merge PR này
```

### Bước 4: Assign reviewer
- Chọn 1 người trong nhóm review
- Đợi họ approve rồi mới MERGE

### Bước 5: Merge
- Click "Merge Pull Request"
- Chọn "Squash and Merge" (gộp tất cả commit thành 1)
- Delete branch sau khi merge

---

## 5. Xử lý Conflict

Khi merge bị conflict:
```
git checkout develop
git pull origin develop
git checkout feature/tên-chức-năng
git merge develop
# Sửa conflict trong các file
git add .
git commit -m "fix: resolve merge conflict"
git push origin feature/tên-chức-năng
```

---

## 6. Quy tắc vàng

1. KHÔNG BAO GIỜ push trực tiếp lên main hoặc develop
2. LUÔN tạo branch mới từ develop
3. LUÔN tạo Pull Request để merge
4. MỖI ngày pull develop về 1 lần để tránh conflict lớn
5. Commit THƯỜNG XUYÊN, đừng để quá nhiều thay đổi trong 1 commit