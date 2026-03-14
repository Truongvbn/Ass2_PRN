## Grand Azure Hotel Platform — System Overview

This document explains the **architecture and major subsystems** of the Grand Azure Hotel Management System, including the new map-based React UI and how it integrates with the existing Razor Pages app.

---

### 1. High-Level Architecture

```text
Browser
  ├─ Razor Pages (HTML, CSS, JS)
  │    ├─ Booking, Reviews, Tickets, Admin, Auth
  │    └─ Layout + Navigation + Design System
  └─ React Mini-App (Vite bundle)
       ├─ RoomsExplorer (map + hotels + rooms)
       └─ HotelLocationMap (small map on Room Detail)

HotelBooking.Web (Presentation)
  ├─ Pages/             # Razor Pages
  ├─ Controllers/       # JSON APIs for React
  ├─ Hubs/              # SignalR hubs
  └─ wwwroot/           # Static assets + React bundle

HotelBooking.Business (Domain & Application)
  ├─ DTOs/AllDtos.cs
  ├─ Services/*Service.cs
  └─ Mappings/MappingProfile.cs

HotelBooking.Data (Persistence)
  ├─ Entities/*.cs
  ├─ Configurations/EntityConfigurations.cs
  ├─ SeedData.cs
  └─ Migrations/
```

Key principles:

- **Strict layering**: Web → Business → Data. Razor Pages and controllers talk to services, never directly to repositories or DbContext.
- **DTO-only boundaries**: Controllers and Razor Pages work with DTOs, not EF entities.
- **ServiceResult pattern**: Services return `ServiceResult<T>` for consistent success/error handling.
- **SignalR for real-time**: Booking, reviews, and tickets are pushed to clients via hubs.

---

### 2. Backend Domain Model (Simplified)

Main entities (in `HotelBooking.Data/Entities`):

- `Hotel`
  - `Name`, `Description`, `Address`, `City`
  - `Latitude`, `Longitude` (geo coordinates)
  - `PhoneNumber`, `Email`
  - `ImageUrl` (primary photo)
  - `Gallery` (JSON string array of extra images)
  - `StarRating`, `IsActive`, `CreatedAt`
  - Navigation: `Rooms`, `StaffAssignments`
- `Room`
  - `HotelId`, `RoomTypeId`
  - `Name`, `Description`, `PricePerNight`, `MaxOccupancy`
  - `ImageUrl`, `Gallery` (JSON array)
  - `Amenities` (JSON array of strings)
  - `IsAvailable`, `IsDeleted`, `RowVersion`
  - Navigation: `Hotel`, `RoomType`, `Bookings`, `Reviews`
- `Booking`, `Payment`, `Review`, `ReviewComment`, `SupportTicket`, `HotelStaff`, `RoomType`

Persistence details:

- Fluent configuration lives in `Configurations/EntityConfigurations.cs`.
- Query filters (`HasQueryFilter`) implement **soft delete** for Rooms, Reviews, ReviewComments.
- Concurrency:
  - `RowVersion` on `Room` and `Booking` prevents conflicting updates (e.g. double booking).
- Legacy data handling:
  - `PaymentConfiguration` uses a custom `ValueConverter` to normalize legacy payment strings (e.g. `"Card"` → `PaymentMethod.CreditCard`).

---

### 3. Services & DTOs

Services (in `HotelBooking.Business.Services`) encapsulate all business logic:

- `RoomService` — search rooms, get room by id, room types
- `HotelService` — CRUD for hotels, staff assignment
- `BookingService` — create/cancel/confirm/complete bookings
- `PaymentService` — handle payment lifecycle
- `ReviewService` — reviews + comments
- `TicketService` — support tickets

DTOs (in `HotelBooking.Business/DTOs/AllDtos.cs`):

- `HotelDto`
  - `id`, `name`, `description`, `city`, `address`
  - `latitude`, `longitude`
  - `phoneNumber`, `email`
  - `imageUrl`, `gallery: string[]`
  - `starRating`, `isActive`, `roomCount`
- `RoomDto`
  - Room info + `hotelId`, `hotelName`
  - `hotelLatitude?`, `hotelLongitude?` (for maps on Room Detail)
  - `description`, `imageUrl`, `gallery: string[]`
  - `amenities` (JSON string), `isAvailable`
  - `averageRating`, `reviewCount`
- `RoomListDto`
  - Lightweight room info for list/map usage, including `gallery: string[]`

Mapping is defined in `Mappings/MappingProfile.cs`:

- Converts `Hotel.Gallery` / `Room.Gallery` from JSON string to `string[]` and vice versa.
- Adds computed fields like `RoomDto.HotelLatitude`/`HotelLongitude`, `HotelDto.RoomCount`.

---

### 4. React Map Integration

The React mini-app lives under:

- `HotelBooking.Web/ClientApp/`
  - Bundled by **Vite** into `wwwroot/react/hotel-map.js` and `hotel-map.css`.
  - Uses **TypeScript**, **Tailwind CSS**, and a small shadcn-style design token setup.

Entry point: `ClientApp/src/main.tsx`:

- Mounts `RoomsExplorer` on `#rooms-explorer-root` (on `/Rooms`).
- Mounts `HotelLocationMap` on `#hotel-location-root` (on `/Rooms/Detail`).

#### RoomsExplorer (map + sidebar)

File: `ClientApp/src/components/RoomsExplorer.tsx`

- **Data sources (via JSON API):**
  - `GET /api/hotels?city=` → `HotelDto[]`
  - `GET /api/hotels/{id}/rooms?checkIn=&checkOut=&guests=` → `RoomListDto[]`
  - `GET /api/geo/provinces` → Vietnam provinces (proxy to `https://provinces.open-api.vn/api/v2/`)

- **Sidebar behavior:**
  - Filters: province/city, check-in, check-out, guests.
  - Hotel list: scrollable cards with hero image, city, stars, room count.
  - Selecting a hotel:
    - Flies the map to the hotel’s coordinates.
    - Loads that hotel’s rooms via `getHotelRooms`.
  - Room list: cards with image, type, occupancy, price, availability, “Details” and “Book” links.

- **Map behavior:**
  - Uses `MapLibre` via a wrapper in `components/ui/map.tsx`.
  - Gold markers styled via Tailwind and design tokens.
  - Popups show hotel photo, name, city, stars, and a “View rooms” button that syncs with the sidebar.

#### HotelLocationMap (room detail)

File: `ClientApp/src/components/HotelLocationMap.tsx`

- Reads `data-lat`, `data-lng`, `data-name` from `#hotel-location-root` in `Rooms/Detail.cshtml`.
- Renders a compact MapLibre map centered on the hotel with a single marker and label.

---

### 5. Razor Pages & Routes

Key Razor pages (under `HotelBooking.Web/Pages`):

- **Public / customer:**
  - `/Rooms` — now **React-driven map UI** for discovery.
  - `/Rooms/Detail` — room detail, gallery, reviews/comments, AI concierge, and hotel location map.
  - `/Booking/Create`, `/Booking/Confirmation`, `/Booking/MyBookings`
  - `/Tickets`, `/Tickets/Create`
  - `/Account/Login`, `/Account/Register`, `/Account/Logout`

- **Admin / staff:**
  - `/Admin/Rooms` (CRUD), `/Admin/Bookings`, `/Admin/Tickets`, `/Admin/Hotels`

Layout file: `Pages/Shared/_Layout.cshtml`

- Provides the top navbar, footer, and loads `site.css`.
- Integrates page-specific `@section Styles` / `@section Scripts` so React CSS/JS can be added only where needed.

---

### 6. Seed Data & Environments

`SeedData.InitializeAsync` handles:

- Roles: `Admin`, `Staff`, `Customer`.
- Users: `admin@hotel.com`, `staff@hotel.com`, three customer accounts (all using password `Hotel@123` by default).
- Hotels:
  - 6 major Vietnamese cities (Hà Nội, Hồ Chí Minh, Đà Nẵng, Hải Phòng, Cần Thơ, Huế).
  - Each with coordinates matching `provinces.open-api.vn`.
  - Real Unsplash images in `ImageUrl` and multiple entries in `Gallery`.
- Rooms:
  - Several rooms per hotel across room types (Standard, Superior, Deluxe, Suite, Presidential).
  - Each room has realistic pricing, amenities JSON, and a multi-image gallery.
- Sample bookings, payments, reviews, comments, and support tickets.

This all runs automatically on first application start via EF Core migrations + seed.

---

### 7. How to Work on the Project

**Chạy ứng dụng:** Xem **HUONG_DAN_CHAY.md** (hướng dẫn đầy đủ bằng tiếng Việt) hoặc **Getting Started** trong **README.md**.

**Backend:**

- Use `dotnet ef` targeting `HotelBooking.Data` with startup project `HotelBooking.Web`.
- Add migrations any time you change entities/configuration.
- Keep all domain logic in the Business layer services; do not access repositories directly from Razor Pages or controllers.

**Frontend React:**

- Work in `HotelBooking.Web/ClientApp`.
- Commands:
  - `npm install` (first time)
  - `npm run dev` (if you want to run Vite separately during frontend work)
  - `npm run build` (bundles into `wwwroot/react` — required for map UI; also hooked into `dotnet publish`).
- Tailwind tokens are scoped under `.react-root` so they **don’t clash** with the main CSS design system.

---

For a deep dive into business flows, see:

- **README.md** — high-level features, routes, getting started.
- **HUONG_DAN_CHAY.md** — hướng dẫn chạy dự án (tiếng Việt).
- **FEATURES.md** — detailed flows (Booking, Reviews, Tickets, AI) and business rules (in Vietnamese).

