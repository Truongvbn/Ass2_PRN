# Grand Azure Hotel Management System

A **.NET 10** web application for premium hotel bookings, real-time support, and administrative control.

Built with **N-Tier Architecture** (Data -> Business -> Web), **ASP.NET Core Razor Pages**, **a React+Vite mini-app for map-based discovery**, **Entity Framework Core**, **SignalR**, and **AutoMapper**.

Key highlights:

- **Interactive Vietnam map on `/Rooms`** — React + MapLibre + Tailwind UI for searching hotels and rooms on a map.
- **Rich galleries with real Unsplash images** for both hotels and rooms (multi-image `Gallery` field).
- **Multi-hotel property system** with scoped staff access and full admin tools.

---

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                   HotelBooking.Web                      │
│  (Razor Pages, SignalR Hubs, CSS Design System, JS)     │
├─────────────────────────────────────────────────────────┤
│                HotelBooking.Business                    │
│  (Services, DTOs, AutoMapper Profiles, ServiceResult)   │
├─────────────────────────────────────────────────────────┤
│                  HotelBooking.Data                      │
│  (EF Core DbContext, Entities, Repositories, SeedData)  │
├─────────────────────────────────────────────────────────┤
│              SQL Server LocalDB                         │
└─────────────────────────────────────────────────────────┘
```

### Design Patterns
- **Repository Pattern** - Generic + domain-specific repos
- **Service Layer** - Business logic with `ServiceResult<T>` return type
- **DTO Pattern** - Decouples entities from presentation
- **AutoMapper** - Entity to DTO mapping
- **Dependency Injection** - All services registered in `Program.cs`

---

## Detailed Features & Flows

### 1. Authentication & Authorization (ASP.NET Identity)

**Flow:**
```
Register -> Login -> Role-based access -> Logout
```

- **3 Roles:** Customer, Staff, Admin
- Custom `ApplicationUser` extends `IdentityUser` with `FullName` and `CreatedAt`
- Role-based page protection via `[Authorize(Roles = "Admin,Staff")]`
- Pages: `/Account/Login`, `/Account/Register`, `/Account/Logout`, `/Account/AccessDenied`
- Password policy: minimum 6 chars, uppercase, lowercase, digit, special char

---

### 2. Room Browsing, Map Search & Discovery

**Flow:**
```
Home Page -> Rooms (map + filters) -> Select hotel pin/card -> See rooms -> Room Detail (reviews + AI concierge)
```

**Features:**
- **React map explorer on `/Rooms`:**
  - Left sidebar: province/city, dates, guests filters; scrollable hotel list; per-hotel room list.
  - Right: MapLibre map of Vietnam with hotel pins and rich popups.
- **Search & Filter:** Filter by city, check-in/check-out, guests via the React sidebar (plus server-side room search).
- **Real-time availability badges:** Green "Available" / red "Unavailable" tags on room cards.
- **Room Detail Page:** Full description, amenities list, image gallery, pricing, reviews, AI concierge, and location map.
- **5 Room Types:** Standard, Superior, Deluxe, Suite, Presidential (seeded).
- **~30 Sample Rooms** pre-seeded across major Vietnamese cities with realistic pricing/amenities.

**Service Methods:**
- `SearchRoomsAsync(hotelId?, roomTypeId?, minPrice?, maxPrice?, minOccupancy?, checkIn?, checkOut?)`
- `GetRoomByIdAsync(id)` - includes average rating + review count + hotel coordinates
- `GetRoomTypesAsync()` - for dropdown filters

---

### 3. Booking Flow

**Flow:**
```
Room Detail -> "Book Now" -> Login (if needed) -> Booking Form -> 
Live Price Calculator -> Submit -> Confirmation Page -> My Bookings
```

**Features:**
- **Live Price Calculator:** JavaScript calculates `nights × pricePerNight` in real-time as user selects dates
- **Guest Count Validation:** Cannot exceed room's `MaxOccupancy`
- **Date Validation:** Check-in must be today or later, check-out must be after check-in
- **Overlap Prevention:** Business layer checks for conflicting bookings on the same room
- **Booking Statuses:** `Pending` -> `Confirmed` -> `Completed` (or `Cancelled`)
- **My Bookings Page:** Lists all user bookings with status badges, option to cancel pending bookings
- **SignalR Real-time:** When a booking is created, admin dashboard updates instantly

**Service Methods:**
- `CreateBookingAsync(dto, userId)` - validates dates, overlap, calculates price
- `GetUserBookingsAsync(userId)` - customer's booking history
- `GetAllBookingsAsync()` - admin view of all bookings
- `ConfirmBookingAsync(id)` - admin confirms a pending booking
- `CancelBookingAsync(id, userId)` - customer cancels their own booking
- `CompleteBookingAsync(id)` - admin marks stay as completed

---

### 4. Payment Processing

**Flow:**
```
Booking Created -> Payment Record Auto-generated -> Admin Confirms -> Payment Marked Paid
```

**Features:**
- Payment records linked 1:1 with bookings
- Payment methods: `CreditCard`, `DebitCard`, `BankTransfer`, `Cash`
- Payment statuses: `Pending` -> `Completed` (or `Failed`, `Refunded`)

**Service Methods:**
- `ProcessPaymentAsync(dto)` - processes payment for a booking
- `GetPaymentByBookingAsync(bookingId)` - retrieves payment details

---

### 5. Reviews & Comments

**Flow:**
```
Complete a Stay -> Room Detail -> Write Review (1-5 stars + text) -> 
Other users see review -> Anyone can comment on reviews
```

**Features:**
- **Review Eligibility:** Only customers with a **Completed** booking for that room can leave a review
- **One Review Per Room Per User:** Enforced by unique index `(RoomId, UserId)`
- **Star Ratings:** 1-5 scale, average calculated and displayed on room cards
- **Threaded Comments:** Any authenticated user can reply to reviews
- **Soft Delete:** Reviews and comments use `IsDeleted` flag
- **SignalR Real-time:** New reviews appear instantly on the room detail page

**Service Methods:**
- `CreateReviewAsync(dto, userId)` - creates review with rating validation
- `UpdateReviewAsync(dto, userId)` - user can edit their own review
- `DeleteReviewAsync(id, userId, isAdmin)` - soft delete (owner or admin)
- `GetRoomReviewsAsync(roomId)` - all reviews with nested comments
- `AddCommentAsync(dto, userId)` - add comment to a review
- `DeleteCommentAsync(id, userId, isAdmin)` - soft delete comment

---

### 6. Support Ticket System

**Flow (Customer):**
```
Tickets Page -> Create Ticket (category + priority + description) -> Track Status
```

**Flow (Staff/Admin):**
```
Admin Tickets -> View Open Tickets -> "Assign to Me" -> 
Update Status (Open -> InProgress -> Resolved -> Closed)
```

**Features:**
- **Categories:** `General`, `Maintenance`, `Housekeeping`, `Billing`, `Complaint`
- **Priorities:** `Low`, `Medium`, `High`, `Critical`
- **Status Workflow:** `Open` -> `InProgress` -> `Resolved` -> `Closed`
- **Self-Assignment:** Staff can "Assign to Me" to take ownership
- **Status Audit:** `CreatedAt`, `UpdatedAt`, `ClosedAt` timestamps tracked
- **SignalR Real-time:** Status changes appear instantly for both customer and staff

**Service Methods:**
- `CreateTicketAsync(dto, userId)` - customer creates ticket
- `GetUserTicketsAsync(userId)` - customer's own tickets
- `GetActiveTicketsAsync()` - all non-closed tickets (staff view)
- `AssignTicketAsync(ticketId, staffId)` - staff assigns themselves
- `UpdateTicketStatusAsync(ticketId, newStatus, userId, isStaff)` - state transition

---

### 7. AI Concierge (Recommendation Engine)

**Flow:**
```
Room Detail Page -> AI Widget -> Enter budget/preferences -> 
Get personalized room recommendations + AI-generated answers
```

**Features:**
- **Room Recommendations:** Filters rooms by budget, guest count, and desired amenities
- **Q&A Assistant:** Answers questions about specific rooms or the hotel in general
- Integrated as a floating widget on the Room Detail page

**Service Methods:**
- `RecommendRoomsAsync(preferences)` - filters and ranks rooms
- `AnswerQuestionAsync(question, roomId?)` - generates contextual answers

---

### 8. Real-time Updates (SignalR)

**3 Dedicated SignalR Hubs:**

| Hub | Events | Used On |
|-----|--------|---------|
| `BookingHub` | New booking created, status changed, room availability updated | Room list, Admin Bookings, My Bookings |
| `ReviewHub` | New review posted, new comment added | Room Detail page |
| `TicketHub` | Ticket created, status updated, ticket assigned | Tickets page, Admin Tickets |

**How it works:**
1. Client-side JavaScript (`signalr-booking.js`, `signalr-review.js`, `signalr-ticket.js`) connects to hubs
2. When a service method fires (e.g., `CreateBookingAsync`), it broadcasts via `IHubContext<T>` using secure targeted messaging (`Clients.User(userId)` or `Clients.Group()`).
3. All connected and authorized browsers receive the update and show a **toast notification** + update the UI

---

### 9. Admin Panel

**Admin has access to 4 management areas:**

#### Admin > Hotels (`/Admin/Hotels`)
- **List all hotels** with name, city, star rating, room count, status
- **Create hotel** - name, address, city, coordinates, phone, email, image, gallery
- **Edit hotel** - update any field, toggle IsActive
- **Staff** - assign staff to hotel (`/Admin/Hotels/Staff?id=X`)

#### Admin > Rooms (`/Admin/Rooms`)
- **List all rooms** with type, price, occupancy, availability status
- **Create new room** - select hotel, type, set price, add amenities, image URL/gallery
- **Edit room** - update any field including toggling availability
- **Delete room** - soft delete (sets `IsDeleted = true`)

#### Admin > Bookings (`/Admin/Bookings`)
- **View all bookings** with customer name, room, dates, price, status
- **Detail** - full booking info and payment (`/Admin/Bookings/Detail?id=X`)
- **Confirm** pending bookings -> changes status to `Confirmed`
- **Complete** confirmed bookings -> changes status to `Completed`
- Real-time updates when new bookings arrive

#### Admin > Tickets (`/Admin/Tickets`)
- **View all active tickets** with priority badges, category, status
- **Assign to Me** - staff takes ownership of a ticket
- **Update Status** - transition through the status workflow
- Shows assigned staff name and timestamps

---

## Technology Stack

| Layer | Technology |
|-------|-----------|
| **Framework** | .NET 10 / ASP.NET Core Razor Pages + React (Vite, TypeScript, Tailwind) |
| **Database** | SQL Server LocalDB + Entity Framework Core (Code-First) |
| **Authentication** | ASP.NET Identity (Roles: Customer, Staff, Admin) |
| **Real-time** | SignalR (3 Hubs: BookingHub, ReviewHub, TicketHub) |
| **Mapping** | AutoMapper |
| **Frontend** | Razor Pages, HTML5, custom luxury CSS design system, JavaScript, React+TypeScript Map UI |

---

## Project Structure

```
HotelBooking/
├── HotelBooking.Data/                   # DATA LAYER
│   ├── Entities/                        # Hotel, HotelStaff, Room, Booking, Payment, Review, etc.
│   ├── Configurations/                  # Fluent API entity configurations
│   ├── Repositories/                    # GenericRepository + domain repos
│   ├── HotelDbContext.cs                # EF Core DbContext with Identity
│   ├── SeedData.cs                      # Roles, users, hotels (6 cities), rooms, bookings, reviews, tickets
│   └── Migrations/                      # EF Core migrations
│
├── HotelBooking.Business/              # BUSINESS LAYER
│   ├── DTOs/AllDtos.cs                  # All record DTOs
│   ├── Mappings/MappingProfile.cs       # AutoMapper profiles
│   └── Services/                        # HotelService, RoomService, BookingService, PaymentService, etc.
│
├── HotelBooking.Web/                   # WEB LAYER
│   ├── Program.cs                       # DI, Identity, SignalR config
│   ├── Controllers/                     # HotelsApiController (JSON API for React map)
│   ├── Hubs/Hubs.cs                     # SignalR hub classes
│   ├── ClientApp/                       # React + Vite + TypeScript (map explorer)
│   │   ├── src/
│   │   │   ├── components/              # RoomsExplorer, HotelLocationMap
│   │   │   ├── api.ts, types.ts
│   │   │   └── main.tsx
│   │   └── package.json                 # npm run build → wwwroot/react/
│   ├── Pages/
│   │   ├── Account/                     # Login, Register, Logout, AccessDenied
│   │   ├── Rooms/                       # Index (React map), Detail (reviews + AI + map)
│   │   ├── Booking/                     # Create, Payment, Confirmation, MyBookings
│   │   ├── Tickets/                     # Index, Create
│   │   └── Admin/                       # Hotels, Rooms CRUD, Bookings, Tickets
│   └── wwwroot/
│       ├── css/site.css                 # Luxury design system
│       ├── js/signalr-*.js              # Real-time client scripts
│       └── react/                       # Vite build output (hotel-map.js, hotel-map.css)
```

---

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/) (for building the React map UI)
- SQL Server LocalDB (included with Visual Studio / SQL Server Express)

### First-Time Setup

1. **Restore .NET packages**
   ```powershell
   cd d:\Code\HotelBooking
   dotnet restore
   ```

2. **Build the React map app** (required for `/Rooms` map and room detail map)
   ```powershell
   cd HotelBooking.Web\ClientApp
   npm install
   npm run build
   cd ..\..
   ```
   Output is written to `HotelBooking.Web/wwwroot/react/` (hotel-map.js, hotel-map.css).

3. **Run the web app**
   ```powershell
   dotnet run --project HotelBooking.Web
   ```
   On first run, EF Core applies migrations and seeds the database (roles, users, hotels, rooms, bookings, reviews, tickets). Open **https://localhost:5xxx** (or the URL shown in the console).

### Optional: EF Core tools (only if you add new migrations)
```powershell
dotnet tool install --global dotnet-ef
# Add design package if missing: dotnet add HotelBooking.Web package Microsoft.EntityFrameworkCore.Design
dotnet ef migrations add YourMigrationName --project HotelBooking.Data --startup-project HotelBooking.Web
```

### Subsequent Runs
```powershell
dotnet run --project HotelBooking.Web
```
Re-run `npm run build` in `ClientApp` only when you change React/TypeScript code.

---

## Demo Accounts

**Password for all:** `Hotel@123`

| Role | Email | Access |
|------|-------|--------|
| Admin | `admin@hotel.com` | Full access: Room CRUD, Booking mgmt, Ticket mgmt |
| Staff | `staff@hotel.com` | View bookings, Assign & resolve tickets |
| Customer | `customer@hotel.com` | Browse, Book, Review, Submit tickets |

---

## Testing SignalR Real-Time

1. **Window 1:** Login as `admin@hotel.com` -> Go to **Admin > Bookings**
2. **Window 2 (Incognito):** Login as `customer@hotel.com` -> **Rooms** -> Book a room
3. Watch Admin window show a **toast notification** instantly - no refresh!
4. Admin clicks **Confirm** -> Customer's **My Bookings** status updates live

Works for **Bookings**, **Reviews**, and **Support Tickets**.

---

## All Page Routes

| Route | Auth Required | Role | Description |
|-------|:---:|------|-------------|
| `/` | No | Any | Home page with hero section |
| `/Rooms` | No | Any | Browse & filter rooms |
| `/Rooms/Detail?id=X` | No | Any | Room details + reviews + AI concierge |
| `/Account/Login` | No | Any | Login form |
| `/Account/Register` | No | Any | Registration form |
| `/Booking/Create?roomId=X` | Yes | Customer | Create booking with live price |
| `/Booking/Payment?bookingId=X` | Yes | Customer | Payment form for booking |
| `/Booking/Confirmation?id=X` | Yes | Customer | Booking confirmation details |
| `/Booking/MyBookings` | Yes | Customer | View & cancel bookings |
| `/Tickets` | Yes | Customer/Staff | View tickets |
| `/Tickets/Create` | Yes | Customer | Submit support ticket |
| `/Admin/Hotels` | Yes | Admin/Staff | List hotels |
| `/Admin/Hotels/Create` | Yes | Admin/Staff | Create hotel |
| `/Admin/Hotels/Edit?id=X` | Yes | Admin/Staff | Edit hotel |
| `/Admin/Hotels/Staff?id=X` | Yes | Admin/Staff | Assign staff to hotel |
| `/Admin/Rooms` | Yes | Admin/Staff | Manage rooms |
| `/Admin/Rooms/Create` | Yes | Admin/Staff | Add new room |
| `/Admin/Rooms/Edit?id=X` | Yes | Admin/Staff | Edit room details |
| `/Admin/Bookings` | Yes | Admin/Staff | Confirm/complete bookings |
| `/Admin/Bookings/Detail?id=X` | Yes | Admin/Staff | Booking & payment detail |
| `/Admin/Tickets` | Yes | Admin/Staff | Assign & resolve tickets |

---

## Database

Connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=GrandAzureHotelDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

- **Auto-created** on first run via EF Core Migrations
- **Auto-seeded** with roles, users, hotels (6 Vietnamese cities), room types, rooms, bookings, payments, reviews, comments, tickets
- Uses `rowversion` for optimistic concurrency on Rooms and Bookings
- Soft delete pattern on Rooms, Reviews, and Comments

---

## More documentation
- **HUONG_DAN_CHAY.md** — Hướng dẫn chạy dự án (tiếng Việt)
- **DOCS_OVERVIEW.md** — Kiến trúc và tích hợp React/API
- **FEATURES.md** — Luồng nghiệp vụ và business rules chi tiết
