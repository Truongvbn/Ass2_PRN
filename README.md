# Grand Azure Hotel Management System

A **.NET 10** web application for premium hotel bookings, real-time support, and administrative control.

Built with **N-Tier Architecture** (Data -> Business -> Web), **ASP.NET Core Razor Pages**, **Entity Framework Core**, **SignalR**, and **AutoMapper**.

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

### 2. Room Browsing & Search

**Flow:**
```
Home Page -> Rooms List (search/filter) -> Room Detail (reviews + AI concierge)
```

**Features:**
- **Search & Filter:** Filter by room type, price range (min/max), occupancy, check-in/check-out dates
- **Real-time availability badges:** Green "Available" / Red "Booked" tags
- **Room Detail Page:** Full description, amenities list, image, pricing
- **Average Rating Display:** Star ratings calculated from completed-stay reviews
- **5 Room Types:** Standard, Deluxe, Suite, Penthouse, Villa (seeded)
- **20 Sample Rooms** pre-seeded with varied pricing and amenities

**Service Methods:**
- `SearchRoomsAsync(roomTypeId?, minPrice?, maxPrice?, minOccupancy?, checkIn?, checkOut?)`
- `GetRoomByIdAsync(id)` - includes average rating + review count
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

**Admin has access to 3 management dashboards:**

#### Admin > Rooms (`/Admin/Rooms`)
- **List all rooms** with type, price, occupancy, availability status
- **Create new room** - select type, set price, add amenities, upload image URL
- **Edit room** - update any field including toggling availability
- **Delete room** - soft delete (sets `IsDeleted = true`)

#### Admin > Bookings (`/Admin/Bookings`)
- **View all bookings** with customer name, room, dates, price, status
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
| **Framework** | .NET 10 / ASP.NET Core Razor Pages |
| **Database** | SQL Server LocalDB + Entity Framework Core (Code-First) |
| **Authentication** | ASP.NET Identity (Roles: Customer, Staff, Admin) |
| **Real-time** | SignalR (3 Hubs: BookingHub, ReviewHub, TicketHub) |
| **Mapping** | AutoMapper |
| **Frontend** | HTML5, Vanilla CSS (Custom Luxury Design System), JavaScript |

---

## Project Structure

```
HotelBooking/
├── HotelBooking.Data/                   # DATA LAYER
│   ├── Entities/                        # Room, Booking, Review, SupportTicket, etc.
│   ├── Configurations/                  # Fluent API entity configurations
│   ├── Repositories/                    # GenericRepository + BookingRepo + ReviewRepo
│   ├── HotelDbContext.cs                # EF Core DbContext with Identity
│   ├── SeedData.cs                      # Seeds roles, users, 20 rooms, bookings, reviews, tickets
│   └── Migrations/                      # Auto-generated EF Core migrations
│
├── HotelBooking.Business/              # BUSINESS LAYER
│   ├── DTOs/AllDtos.cs                  # All record DTOs
│   ├── Mappings/MappingProfile.cs       # AutoMapper profiles
│   └── Services/                        # RoomService, BookingService, ReviewService, etc.
│
├── HotelBooking.Web/                   # WEB LAYER
│   ├── Program.cs                       # DI, Identity, SignalR config
│   ├── Hubs/Hubs.cs                     # SignalR hub classes
│   ├── Pages/
│   │   ├── Account/                     # Login, Register, Logout, AccessDenied
│   │   ├── Rooms/                       # Index (list), Detail (view + reviews)
│   │   ├── Booking/                     # Create, Confirmation, MyBookings
│   │   ├── Tickets/                     # Index (list), Create
│   │   └── Admin/                       # Rooms CRUD, Bookings mgmt, Tickets mgmt
│   └── wwwroot/
│       ├── css/site.css                 # Luxury design system
│       └── js/signalr-*.js             # Real-time client scripts
```

---

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server LocalDB (included with Visual Studio)

### First-Time Setup

```powershell
cd d:\Code\HotelBooking

# Install EF Core tools
dotnet tool install --global dotnet-ef

# Add Design package
dotnet add HotelBooking.Web package Microsoft.EntityFrameworkCore.Design

# Create migration
dotnet ef migrations add InitialCreate --project HotelBooking.Data --startup-project HotelBooking.Web

# Run (auto-applies migrations + seeds data)
dotnet run --project HotelBooking.Web
```

### Subsequent Runs
```powershell
dotnet run --project HotelBooking.Web
```

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
| `/Booking/Confirmation?id=X` | Yes | Customer | Booking confirmation details |
| `/Booking/MyBookings` | Yes | Customer | View & cancel bookings |
| `/Tickets` | Yes | Customer/Staff | View tickets |
| `/Tickets/Create` | Yes | Customer | Submit support ticket |
| `/Admin/Rooms` | Yes | Admin/Staff | Manage rooms |
| `/Admin/Rooms/Create` | Yes | Admin/Staff | Add new room |
| `/Admin/Rooms/Edit?id=X` | yes | Admin/Staff | Edit room details |
| `/Admin/Bookings` | Yes | Admin/Staff | Confirm/complete bookings |
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
- **Auto-seeded** with roles, 5 users, 5 room types, 20 rooms, 10 bookings, 15 reviews, 17 comments, 5 tickets
- Uses `rowversion` for optimistic concurrency on Rooms and Bookings
- Soft delete pattern on Rooms, Reviews, and Comments
