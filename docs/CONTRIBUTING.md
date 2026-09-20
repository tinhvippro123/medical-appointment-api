# Huong Dan Commit va Pull Request

## 1. Quy trinh lam viec hang ngay

```
Buoc 1: Pull code moi nhat
  git checkout develop
  git pull origin develop

Buoc 2: Tao branch moi tu develop
  git checkout -b feature/ten-chuc-nang

Buoc 3: Code xong, commit
  git add .
  git commit -m "feat: mo ta ngan gon"

Buoc 4: Push len GitHub
  git push origin feature/ten-chuc-nang

Buoc 5: Vao GitHub tao Pull Request
  Base: develop  <--  Compare: feature/ten-chuc-nang

Buoc 6: Doi team review, approve roi MERGE
```

---

## 2. Dat ten Branch

```
feature/auth-login          <- Chuc nang moi
feature/department-crud     <- Chuc nang moi
fix/login-token-expired     <- Sua loi
refactor/doctor-service     <- Tai cau truc code
```

### Vi du cu the cho tung nguoi

```
Tinh:   feature/auth-register
        feature/auth-login
        feature/auth-jwt
        feature/user-management
        feature/role-permission

Triet:  feature/department-crud
        feature/department-image-upload
        feature/doctor-crud-admin

Thinh:  feature/home-screen
        feature/doctor-list
        feature/doctor-detail
        feature/doctor-search

Thuan:  feature/cart-add-remove
        feature/cart-update
        feature/checkout
        feature/order-history
        feature/payment-vnpay
```

---

## 3. Viet Commit Message

### Format

```
<loai>: <mo ta ngan gon bang tieng Viet hoac tieng Anh>
```

### Cac loai commit

| Loai | Khi nao dung | Vi du |
|------|-------------|-------|
| feat | Them chuc nang moi | feat: them API dang ky nguoi dung |
| fix | Sua loi | fix: sua loi token het han khi login |
| refactor | Sua code nhung khong doi chuc nang | refactor: toi uu DoctorService |
| docs | Cap nhat tai lieu | docs: cap nhat README |
| style | Format code, sua loi chinh ta | style: format code AuthController |
| chore | Cong viec setup, config | chore: them package Dio vao pubspec |
| test | Them/sua test | test: them unit test cho AuthService |

### Vi du commit tot

```
feat: them API POST /api/auth/register
feat: them man hinh dang nhap
fix: sua loi crash khi danh sach bac si rong
refactor: tach BookingService thanh cac method nho
docs: them API endpoints vao README
chore: cau hinh JWT trong appsettings.json
```

### Vi du commit XAU (khong nen)

```
update code              <- Qua chung chung
fix bug                  <- Bug gi? O dau?
asdkjahsd               <- Vo nghia
them nhieu thu           <- Them cai gi?
```

---

## 4. Tao Pull Request tren GitHub

### Buoc 1: Vao GitHub repo, click "Pull Requests" -> "New Pull Request"

### Buoc 2: Chon branch

```
base: develop  <----  compare: feature/auth-login
```

### Buoc 3: Dien thong tin PR

**Tieu de PR:**
```
[Tinh] feat: Them chuc nang dang nhap va dang ky
```

**Noi dung PR (copy mau nay):**
```
## Mo ta
- Them API POST /api/auth/register (dang ky)
- Them API POST /api/auth/login (dang nhap, tra JWT token)
- Them man hinh Login va Register tren Flutter

## Checklist
- [ ] Code chay duoc, khong bi loi
- [ ] Da test bang Swagger/Postman
- [ ] Khong conflict voi branch develop

## Screenshot (neu co UI)
(Dan hinh man hinh o day)

## Ghi chu
- Can merge PR cua Tinh (setup DbContext) truoc khi merge PR nay
```

### Buoc 4: Assign reviewer
- Chon 1 nguoi trong nhom review
- Doi ho approve roi moi MERGE

### Buoc 5: Merge
- Click "Merge Pull Request"
- Chon "Squash and Merge" (gop tat ca commit thanh 1)
- Delete branch sau khi merge

---

## 5. Xu ly Conflict

Khi merge bi conflict:
```
git checkout develop
git pull origin develop
git checkout feature/ten-chuc-nang
git merge develop
# Sua conflict trong cac file
git add .
git commit -m "fix: resolve merge conflict"
git push origin feature/ten-chuc-nang
```

---

## 6. Quy tac vang

1. KHONG BAO GIO push truc tiep len main hoac develop
2. LUON tao branch moi tu develop
3. LUON tao Pull Request de merge
4. MOI ngay pull develop ve 1 lan de tranh conflict lon
5. Commit THUONG XUYEN, dung de qua nhieu thay doi trong 1 commit
