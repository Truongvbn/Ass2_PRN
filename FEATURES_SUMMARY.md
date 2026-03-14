# Tổng kết tính năng — Grand Azure Hotel

## Đã có sẵn

1. **Đăng nhập / Phân quyền** — ASP.NET Identity, 3 vai trò: Customer, Staff, Admin.
2. **Khách sạn đa property** — Entity Hotel, HotelStaff, admin CRUD + gán nhân sự (Admin/Hotels, Staff).
3. **Phòng & loại phòng** — Room gắn Hotel, 5 loại phòng, gallery ảnh, amenities JSON, Admin Rooms CRUD.
4. **Tìm phòng & bản đồ** — React + MapLibre trên `/Rooms`: lọc theo tỉnh/thành, ngày, số khách; danh sách khách sạn + phòng; bản đồ Việt Nam với pin và popup.
5. **Chi tiết phòng** — Mô tả, gallery, tiện nghi, giá, đánh giá, bản đồ vị trí khách sạn (HotelLocationMap).
6. **Đặt phòng** — Tạo booking, tính giá theo đêm, kiểm tra trùng lịch, trạng thái Pending → Confirmed → Completed/Cancelled; My Bookings, hủy đặt.
7. **Thanh toán** — Payment 1:1 với Booking, phương thức (CreditCard, DebitCard, BankTransfer, Cash), trạng thái Pending/Completed.
8. **Đánh giá & bình luận** — Chỉ khách đã ở xong mới review; 1–5 sao; comment trên review; soft delete; real-time qua SignalR.
9. **Hỗ trợ (ticket)** — Tạo ticket, gán cho staff, luồng trạng thái Open → InProgress → Resolved → Closed; real-time.
10. **AI Concierge** — Gợi ý phòng theo ngân sách/sở thích, Q&A trên trang chi tiết phòng (mock).
11. **Real-time (SignalR)** — BookingHub, ReviewHub, TicketHub; toast + cập nhật UI khi có thay đổi.
12. **Admin** — Rooms CRUD, Bookings (confirm/complete), Tickets (assign/status), Hotels CRUD + Staff.
13. **API cho React** — `GET /api/hotels`, `GET /api/hotels/{id}/rooms`, proxy `/api/geo/provinces`.
14. **Seed data** — Roles, users, khách sạn 6 thành phố VN, phòng, gallery Unsplash, bookings, reviews, tickets.
15. **Tests** — BookingServiceScopingTests, HotelServiceTests, RoomServiceScopingTests.
16. **Booking expiration** — BookingExpirationService (background xử lý hết hạn đặt chỗ).

---

*Tổng kết ngày 14/03/2025 — dựa trên README.md và DOCS_OVERVIEW.md.*
