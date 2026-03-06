# 📋 Tài Liệu Mô Tả Tính Năng — Grand Azure Hotel Management System

> Tài liệu này mô tả chi tiết **tất cả các tính năng** đã được triển khai trong hệ thống, bao gồm 3 luồng nghiệp vụ chính, các business rules, SignalR real-time events, và chức năng quản trị.

---

## 🏗️ Kiến trúc 3 Lớp (N-Tier)

```
HotelBooking.Web  →  HotelBooking.Business  →  HotelBooking.Data
  (Razor Pages,       (IService, DTOs,            (IRepository,
   SignalR Hubs, JS)    AutoMapper, Rules)          Entities, DbContext)
```

### Nguyên tắc DI (Dependency Injection)
| ✅ Đúng | ❌ Sai |
|---------|--------|
| PageModel chỉ inject `IService` | PageModel inject `IRepository` hoặc `DbContext` |
| DI container inject `IRoomService` | `new RoomService()` trong code |
| Service trả DTO, map bằng AutoMapper | Trả Entity từ Service cho PageModel |
| Hub chỉ broadcast, Service gọi qua `IHubContext` | Hub gọi Repository trực tiếp |

### SignalR Broadcast Pattern
```
PageModel.OnPost() → IService.Method() → IRepository → DB
                                        ↓
                              IHubContext<THub>.Clients.All.SendAsync()
                                        ↓
                              JS Client nhận event → cập nhật DOM
```

---

## 📌 Luồng 1: Booking & Thanh Toán

### Flow
```
Trang chủ → Danh sách phòng (search/filter) → Chi tiết phòng → 
"Book Now" → Login (nếu chưa) → Form đặt phòng (live price) → 
Submit → Trang xác nhận → My Bookings (xem/hủy)
```

### Tính năng đã triển khai

| # | Tính năng | Trang | Mô tả |
|---|-----------|-------|-------|
| 1 | **Tìm kiếm & lọc phòng** | `/Rooms` | Lọc theo loại phòng, giá min/max, số khách, ngày check-in/check-out |
| 2 | **Badge trạng thái phòng** | `/Rooms` | Badge xanh "Available" / đỏ "Booked" hiển thị real-time |
| 3 | **Chi tiết phòng** | `/Rooms/Detail?id=X` | Mô tả, ảnh, giá, tiện nghi, rating trung bình, danh sách review |
| 4 | **Live Price Calculator** | `/Booking/Create` | JavaScript tính `số đêm × giá/đêm` tự động khi chọn ngày |
| 5 | **Đặt phòng** | `/Booking/Create` | Form nhập ngày + số khách, validate phía server |
| 6 | **Trang xác nhận** | `/Booking/Confirmation` | Hiển thị chi tiết booking sau khi đặt thành công |
| 7 | **My Bookings** | `/Booking/MyBookings` | Danh sách booking của user, nút hủy booking Pending |
| 8 | **Admin quản lý booking** | `/Admin/Bookings` | Xem tất cả booking, nút Confirm/Complete |

### Business Rules đã enforce

| Rule | Cách validate |
|------|---------------|
| CheckIn < CheckOut | Service validate trước khi tạo booking |
| CheckIn ≥ Today | Không cho đặt ngày quá khứ |
| NumberOfGuests ≤ Room.MaxOccupancy | So sánh với Room entity |
| **Không trùng ngày** | Query DB kiểm tra overlapping booking cùng room |
| Room.IsAvailable && !Room.IsDeleted | Không đặt phòng đã xóa/ẩn |
| TotalPrice = PricePerNight × số đêm | Service tính, không tin client |
| Cancel chỉ khi Status = Pending/Confirmed | State machine enforce |

### Booking Status Machine
```
Pending → Confirmed → Completed
   ↓
Cancelled
```

### SignalR Events (BookingHub)
| Event | Khi nào | Ai nhận |
|-------|---------|---------|
| `BookingCreated(BookingDto)` | Khách đặt phòng | Tất cả — badge phòng cập nhật |
| `BookingStatusChanged(id, status)` | Admin confirm/complete | Khách thấy status đổi |
| `BookingCancelled(id)` | Khách hủy | Tất cả — phòng available lại |

---

## 📌 Luồng 2: Review Rating & Commenting

### Flow
```
Hoàn thành booking (Completed) → Vào chi tiết phòng → 
Viết review (1-5 sao + nội dung) → Submit → 
Người khác xem review → Comment trả lời
```

### Tính năng đã triển khai

| # | Tính năng | Trang | Mô tả |
|---|-----------|-------|-------|
| 1 | **Xem review** | `/Rooms/Detail` | Danh sách review với star rating, nội dung, tên tác giả |
| 2 | **Rating trung bình** | `/Rooms`, `/Rooms/Detail` | Hiển thị trên card phòng và trang chi tiết |
| 3 | **Viết review** | `/Rooms/Detail` | Form rating 1-5 sao + textarea (chỉ hiện khi đủ điều kiện) |
| 4 | **Comment** | `/Rooms/Detail` | Ai đã login cũng có thể comment dưới review |
| 5 | **Edit/Delete review** | Service layer | Chỉ owner hoặc Admin được sửa/xóa |

### Business Rules đã enforce

| Rule | Cách validate |
|------|---------------|
| Rating 1-5 | Validate ở Service |
| **Chỉ review nếu đã ở** (Booking Completed) | Query bookings của user cho room đó |
| **1 review / user / room** | Unique index `(RoomId, UserId)` WHERE `IsDeleted = 0` |
| Content không rỗng | Validate length |
| Chỉ owner được sửa/xóa review | So sánh UserId |
| Comment: ai cũng được comment | Chỉ cần đã login |
| Edit/Delete comment: chỉ owner hoặc Admin | Check role + ownership |
| Soft delete | `IsDeleted = true`, không xóa khỏi DB |

### SignalR Events (ReviewHub)
| Event | Khi nào | Ai nhận |
|-------|---------|---------|
| `ReviewCreated` | Khách viết review | Tất cả đang xem phòng |
| `ReviewUpdated` | Khách sửa review | Tất cả đang xem phòng |
| `ReviewDeleted` | Xóa review | Tất cả đang xem phòng |
| `CommentAdded` | Có comment mới | Tất cả đang xem phòng |

---

## 📌 Luồng 3: Support Tickets

### Flow (Khách hàng)
```
Tickets → "Create Ticket" → Chọn Category + ưu tiên → 
Nhập mô tả → Submit → Xem trạng thái ticket
```

### Flow (Staff/Admin)
```
Admin Tickets → Xem danh sách ticket open → "Assign to Me" → 
Cập nhật status (Open → InProgress → Resolved → Closed)
```

### Tính năng đã triển khai

| # | Tính năng | Trang | Mô tả |
|---|-----------|-------|-------|
| 1 | **Tạo ticket** | `/Tickets/Create` | Form nhập category, subject, description |
| 2 | **Xem ticket của mình** | `/Tickets` | Customer xem danh sách ticket đã tạo |
| 3 | **Staff xem tất cả ticket** | `/Admin/Tickets` | Xem mọi ticket active với priority badge |
| 4 | **Assign to Me** | `/Admin/Tickets` | Staff tự nhận xử lý ticket |
| 5 | **Cập nhật status** | `/Admin/Tickets` | Chuyển đổi trạng thái theo state machine |
| 6 | **Timestamp tracking** | Tự động | `CreatedAt`, `UpdatedAt`, `ClosedAt` |

### Ticket Status Machine
```
Open → InProgress → Resolved → Closed
  ↓                    ↓
  → Closed (customer)  → Open (customer reopen)
```

### Business Rules đã enforce

| Rule | Cách validate |
|------|---------------|
| Customer chỉ tạo/đóng ticket của mình | Check ownership |
| Staff/Admin assign ticket | Chỉ role Staff/Admin |
| **State transitions hợp lệ** | Open→InProgress (Staff), InProgress→Resolved (Staff), Resolved→Closed/Open |
| Category bắt buộc | General, Maintenance, Housekeeping, Billing, Complaint |
| Priority levels | Low, Medium, High, Critical |

### SignalR Events (TicketHub)
| Event | Khi nào | Ai nhận |
|-------|---------|---------|
| `TicketCreated(TicketDto)` | Khách tạo ticket | Staff dashboard thấy ngay |
| `TicketAssigned(id, staffName)` | Staff nhận ticket | Customer thấy ai xử lý |
| `TicketStatusChanged(id, status)` | Đổi trạng thái | Cả hai bên thấy |
| `TicketClosed(id)` | Đóng ticket | Ticket biến mất khỏi active list |

---

## 📌 Tính năng bổ sung

### 🤖 AI Concierge (Recommendation Engine)
| # | Tính năng | Trang | Mô tả |
|---|-----------|-------|-------|
| 1 | **Gợi ý phòng** | `/Rooms/Detail` | Nhập ngân sách, số khách, tiện nghi → gợi ý phòng phù hợp |
| 2 | **Hỏi đáp AI** | `/Rooms/Detail` | Trả lời câu hỏi về phòng hoặc khách sạn |

### 🔐 Authentication & Authorization
| # | Tính năng | Trang | Mô tả |
|---|-----------|-------|-------|
| 1 | **Đăng ký tài khoản** | `/Account/Register` | Tạo tài khoản Customer mới |
| 2 | **Đăng nhập** | `/Account/Login` | Email + password |
| 3 | **Đăng xuất** | `/Account/Logout` | Redirect về trang chủ |
| 4 | **Phân quyền** | Tất cả trang Admin | `[Authorize(Roles = "Admin,Staff")]` |
| 5 | **Access Denied** | `/Account/AccessDenied` | Khi truy cập trang không có quyền |

### 👑 Admin Panel — CRUD Phòng
| # | Tính năng | Trang | Mô tả |
|---|-----------|-------|-------|
| 1 | **Danh sách phòng** | `/Admin/Rooms` | Bảng hiển thị tất cả phòng |
| 2 | **Thêm phòng** | `/Admin/Rooms/Create` | Form thêm phòng mới (loại, giá, mô tả, ảnh, tiện nghi) |
| 3 | **Sửa phòng** | `/Admin/Rooms/Edit?id=X` | Update bất kỳ trường nào, toggle availability |
| 4 | **Xóa phòng** | `/Admin/Rooms` | Soft delete (IsDeleted = true) |

---

## 🗃️ Entity Relationships

```
User ──┬── creates ──→ Booking ──── has ──→ Payment
       ├── writes  ──→ Review  ──── has ──→ ReviewComment
       ├── creates ──→ SupportTicket
       └── assigned to → SupportTicket (Staff)

Room ──┬── has ──→ Booking
       ├── has ──→ Review
       └── belongs to → RoomType
```

### Concurrency & Audit
- Tất cả entity có `CreatedAt`, `UpdatedAt`
- `Room`, `Booking` có `RowVersion` (optimistic concurrency) — tránh double-booking
- Soft delete cho `Room`, `Review`, `ReviewComment` (field `IsDeleted`)
- `SupportTicket` không soft delete, dùng status Closed

---

## 🌱 Seed Data (Tự động tạo khi chạy lần đầu)

| Loại | Số lượng | Chi tiết |
|------|----------|----------|
| **RoomTypes** | 5 | Standard, Deluxe, Suite, Penthouse, Villa |
| **Rooms** | 20 | Đầy đủ mô tả, giá, tiện nghi, ảnh |
| **Users** | 3 | admin@hotel.com, staff@hotel.com, customer@hotel.com |
| **Bookings** | 4 | Nhiều status: Pending, Confirmed, Completed |
| **Payments** | 3 | Liên kết với bookings |
| **Reviews** | 2 | Kèm star rating |
| **Comments** | 3 | Dưới các reviews |
| **Tickets** | 3 | Open, InProgress, Resolved |

**Mật khẩu tất cả tài khoản:** `Hotel@123`

---

## 🛠️ Technology Stack

| Thành phần | Công nghệ |
|------------|-----------|
| Framework | .NET 10 / ASP.NET Core Razor Pages |
| Database | SQL Server LocalDB + Entity Framework Core (Code-First) |
| Authentication | ASP.NET Identity (3 Roles) |
| Real-time | SignalR (3 Hubs: BookingHub, ReviewHub, TicketHub) |
| Mapping | AutoMapper |
| Frontend | HTML5, CSS3 (Custom Luxury Design System), JavaScript |
| Pattern | Repository, Service Layer, DTO, ServiceResult\<T\> |
