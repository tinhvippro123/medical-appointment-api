# Rooms API

Base URL mặc định: `http://localhost:5041`.

| Method | URL | Quyền | Kết quả |
|---|---|---|---|
| GET | /api/rooms | Công khai | 200, danh sách phòng đang hoạt động |
| GET | /api/rooms?departmentId=1 | Công khai | 200, lọc phòng theo chuyên khoa |
| GET | /api/rooms/{id} | Công khai | 200, chi tiết phòng |
| POST | /api/rooms | ADMIN | 201, phòng mới và header Location |
| PUT | /api/rooms/{id} | ADMIN | 200, phòng đã cập nhật |
| DELETE | /api/rooms/{id} | ADMIN | 204, xóa mềm IsActive=false |

## Postman

POST/PUT: chọn Authorization → Bearer Token → nhập token đăng nhập của tài khoản ADMIN.
Body → raw → JSON:

```json
{
  "roomNumber": "A101",
  "floor": 1,
  "departmentId": 1
}
```

Thay departmentId bằng mã chuyên khoa đang hoạt động. PUT gửi đầy đủ cả ba trường.
RoomNumber được loại bỏ khoảng trắng đầu/cuối khi lưu, tối đa 20 ký tự.
Không cần migration mới vì bảng Rooms đã có sẵn.

## Các ca kiểm tra

- Tạo phòng, dùng id trả về để GET, PUT rồi DELETE.
- Sau DELETE: GET chi tiết, PUT và DELETE lại trả 404; danh sách không còn phòng đó.
- RoomNumber rỗng/chỉ có khoảng trắng/quá 20 ký tự: 400.
- Floor <= 0, DepartmentId <= 0 hoặc thiếu trường bắt buộc: 400.
- Chuyên khoa không tồn tại hoặc đã xóa mềm: 404 DEPARTMENT_NOT_FOUND.
- Phòng không tồn tại hoặc đã xóa mềm: 404 ROOM_NOT_FOUND.
- POST/PUT/DELETE không có token: 401; token hợp lệ không có role ADMIN: 403.
- Bộ lọc không có phòng phù hợp: 200 và mảng rỗng.
