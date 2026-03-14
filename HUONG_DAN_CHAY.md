# Hướng dẫn chạy dự án — Grand Azure Hotel

Hướng dẫn cài đặt môi trường và chạy ứng dụng web **Grand Azure Hotel Management System** trên máy local.

---

## 1. Yêu cầu hệ thống

| Thành phần | Yêu cầu |
|------------|--------|
| **.NET** | .NET 10 SDK — [Tải tại đây](https://dotnet.microsoft.com/download) |
| **Node.js** | Phiên bản 18 trở lên (để build giao diện bản đồ React) — [Tải tại đây](https://nodejs.org/) |
| **Cơ sở dữ liệu** | SQL Server LocalDB (đi kèm Visual Studio hoặc [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads)) |

Kiểm tra nhanh:
```powershell
dotnet --version   # Ví dụ: 10.x.x
node --version     # Ví dụ: v20.x.x
npm --version      # Ví dụ: 10.x.x
```

---

## 2. Các bước chạy lần đầu

### Bước 1: Mở thư mục dự án
```powershell
cd d:\Code\HotelBooking
```
*(Thay đường dẫn bằng thư mục bạn clone/đặt project.)*

### Bước 2: Khôi phục package .NET
```powershell
dotnet restore
```

### Bước 3: Build giao diện React (bản đồ phòng / khách sạn)
Giao diện bản đồ trên trang **Phòng** và bản đồ vị trí trên **Chi tiết phòng** dùng React + Vite. Cần build một lần để tạo file trong `wwwroot/react/`.

```powershell
cd HotelBooking.Web\ClientApp
npm install
npm run build
cd ..\..
```

Sau khi chạy xong, trong `HotelBooking.Web\wwwroot\react\` sẽ có `hotel-map.js` và `hotel-map.css`.

### Bước 4: Chạy ứng dụng
```powershell
dotnet run --project HotelBooking.Web
```

Lần chạy đầu tiên:
- EF Core tự áp dụng migrations và tạo database.
- Seed data tự chạy: roles, user mẫu, khách sạn (6 thành phố), phòng, booking, review, ticket.

Khi chạy xong, mở trình duyệt theo địa chỉ in trên console (thường dạng **https://localhost:5xxx** hoặc **http://localhost:5xxx**).

---

## 3. Chạy những lần sau

Chỉ cần:
```powershell
cd d:\Code\HotelBooking
dotnet run --project HotelBooking.Web
```

Chỉ cần chạy lại `npm run build` trong `HotelBooking.Web\ClientApp` khi bạn sửa code React/TypeScript (component bản đồ, API client, v.v.).

---

## 4. Tài khoản đăng nhập thử nghiệm

Mật khẩu chung: **`Hotel@123`**

| Vai trò   | Email               | Mô tả ngắn |
|-----------|---------------------|------------|
| Admin     | `admin@hotel.com`   | Quản lý khách sạn, phòng, đặt phòng, ticket |
| Staff     | `staff@hotel.com`   | Xem booking, gán và xử lý ticket |
| Customer  | `customer@hotel.com`| Xem phòng, đặt phòng, đánh giá, tạo ticket |

---

## 5. Một số lệnh hữu ích

### Build lại React (sau khi sửa ClientApp)
```powershell
cd HotelBooking.Web\ClientApp
npm run build
cd ..\..
```

### Chạy ở chế độ development (Vite dev server) — tùy chọn
Nếu bạn đang sửa giao diện React và muốn hot-reload:
- Cửa sổ 1: `dotnet run --project HotelBooking.Web`
- Cửa sổ 2: `cd HotelBooking.Web\ClientApp` rồi `npm run dev`

*(Cần cấu hình proxy / base URL nếu dùng đồng thời; mặc định production dùng bundle trong `wwwroot/react`.)*

### Tạo migration mới (khi đổi model/entity)
```powershell
dotnet tool install --global dotnet-ef
dotnet ef migrations add TenMigration --project HotelBooking.Data --startup-project HotelBooking.Web
```

### Cập nhật database sau khi thêm migration
```powershell
dotnet ef database update --project HotelBooking.Data --startup-project HotelBooking.Web
```
Hoặc đơn giản chạy lại ứng dụng; nhiều cấu hình mặc định tự áp dụng migration khi start.

---

## 6. Chuỗi kết nối database

Trong `HotelBooking.Web/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=GrandAzureHotelDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

Database **GrandAzureHotelDb** sẽ được tạo tự động lần chạy đầu (LocalDB).

---

## 7. Xử lý lỗi thường gặp

| Lỗi | Gợi ý xử lý |
|-----|-------------|
| Không tìm thấy `dotnet` | Cài .NET 10 SDK và mở lại terminal. |
| Không tìm thấy `npm` / `node` | Cài Node.js 18+ và mở lại terminal. |
| LocalDB không chạy | Cài SQL Server Express (có LocalDB) hoặc dùng Visual Studio (đi kèm LocalDB). |
| Trang `/Rooms` không có bản đồ | Chạy `npm run build` trong `HotelBooking.Web\ClientApp` rồi chạy lại `dotnet run`. |
| Port bị chiếm | Đổi port trong `HotelBooking.Web/Properties/launchSettings.json` hoặc tắt app đang dùng port đó. |

---

Tài liệu tổng quan tính năng và kiến trúc: **README.md**, **DOCS_OVERVIEW.md**, **FEATURES.md**.
