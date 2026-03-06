# 🏨 Grand Azure Hotel Management System

A luxurious, full-stack **.NET 10** web application designed for premium hotel bookings, real-time ticket support, and seamless administrative control.

Built with a strict **N-Tier Architecture** (Data → Business → Web), utilizing **ASP.NET Core Razor Pages**, **Entity Framework Core**, **SignalR**, and **AutoMapper**.

---

## 📐 Architecture Overview

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

### Design Patterns Used
- **Repository Pattern** — Generic + domain-specific repositories for data access
- **Service Layer Pattern** — Business logic isolated in services with `ServiceResult<T>` return type
- **DTO Pattern** — Data Transfer Objects decouple entities from the presentation layer
- **AutoMapper** — Automatic entity-to-DTO mapping
- **Dependency Injection** — All services and repositories registered via DI in `Program.cs`

---

## 🌟 Key Features

### 🛎️ For Customers
| Feature | Description |
|---------|-------------|
| **Luxury UI/UX** | Premium dark navy/gold theme with glassmorphism, smooth transitions, and responsive design |
| **Smart Room Browsing** | Search, filter by type/price, real-time availability badges |
| **Instant Booking** | Live price calculator, date selection, immediate booking confirmation |
| **AI Concierge** | Room recommendations based on budget and amenity preferences |
| **Reviews & Comments** | Star ratings + text reviews (only for completed stays), threaded comments |
| **Support Tickets** | Submit requests (e.g. extra towels, late checkout) and track resolution in real-time |

### 🛡️ For Staff & Admins
| Feature | Description |
|---------|-------------|
| **Live Dashboards (SignalR)** | Instant toast notifications when customers book, review, or submit tickets — no page reload |
| **Booking Management** | Confirm pending bookings or mark as completed |
| **Ticket Queue** | Assign yourself to tickets, transition status (Open → In Progress → Resolved) |
| **Room CRUD** | Add rooms, edit pricing/amenities, toggle availability, manage room types |

---

## 📁 Project Structure

```
HotelBooking/
├── HotelBooking.slnx                    # Solution file
├── README.md                            # This file
│
├── HotelBooking.Data/                   # DATA LAYER
│   ├── Entities/                        # EF Core entity classes
│   │   ├── ApplicationUser.cs           # Extended IdentityUser
│   │   ├── Room.cs, RoomType.cs
│   │   ├── Booking.cs, Payment.cs
│   │   ├── Review.cs, ReviewComment.cs
│   │   └── SupportTicket.cs
│   ├── Configurations/                  # Fluent API configurations
│   ├── Repositories/                    # Generic + specific repos
│   │   ├── GenericRepository.cs
│   │   ├── BookingRepository.cs
│   │   └── ReviewRepository.cs
│   ├── HotelDbContext.cs                # EF Core DbContext
│   ├── SeedData.cs                      # Auto-seeds roles, users, rooms, bookings
│   └── Migrations/                      # EF Core migrations (auto-generated)
│
├── HotelBooking.Business/              # BUSINESS LAYER
│   ├── DTOs/
│   │   └── AllDtos.cs                   # All record DTOs (Room, Booking, Review, Ticket, etc.)
│   ├── Mappings/
│   │   └── MappingProfile.cs            # AutoMapper entity↔DTO mappings
│   └── Services/
│       ├── Interfaces/IServices.cs      # Service contracts
│       ├── RoomService.cs
│       ├── BookingService.cs
│       ├── ReviewService.cs
│       ├── TicketService.cs
│       ├── PaymentService.cs
│       └── AIAssistantService.cs        # AI room recommendation engine
│
├── HotelBooking.Web/                   # WEB LAYER (Presentation)
│   ├── Program.cs                       # DI, Identity, SignalR, middleware config
│   ├── Hubs/Hubs.cs                     # SignalR hub marker classes
│   ├── Pages/
│   │   ├── Index.cshtml                 # Home page (hero + features)
│   │   ├── Account/                     # Login, Register, Logout, AccessDenied
│   │   ├── Rooms/
│   │   │   ├── Index.cshtml             # Room listing + search/filter
│   │   │   └── Detail.cshtml            # Room detail + reviews + AI concierge
│   │   ├── Booking/
│   │   │   ├── Create.cshtml            # Booking form + live price calculator
│   │   │   ├── Confirmation.cshtml      # Booking confirmation
│   │   │   └── MyBookings.cshtml        # User's booking history + cancel
│   │   ├── Tickets/
│   │   │   ├── Index.cshtml             # User/Staff ticket list
│   │   │   └── Create.cshtml            # New ticket form
│   │   └── Admin/
│   │       ├── Rooms/Index, Create, Edit # Room management CRUD
│   │       ├── Bookings/Index.cshtml     # Confirm/complete bookings
│   │       └── Tickets/Index.cshtml      # Assign/resolve tickets
│   └── wwwroot/
│       ├── css/site.css                 # Full luxury design system
│       └── js/
│           ├── signalr-booking.js       # Real-time booking updates
│           ├── signalr-review.js        # Real-time review updates
│           └── signalr-ticket.js        # Real-time ticket updates
```

---

## 🛠️ Technology Stack

| Layer | Technology |
|-------|-----------|
| **Framework** | .NET 10 / ASP.NET Core Razor Pages |
| **Database** | SQL Server LocalDB + Entity Framework Core (Code-First) |
| **Authentication** | ASP.NET Identity (Roles: Customer, Staff, Admin) |
| **Real-time** | SignalR (3 Hubs: BookingHub, ReviewHub, TicketHub) |
| **Mapping** | AutoMapper |
| **Frontend** | HTML5, Vanilla CSS (Custom Design System), JavaScript |

---

## 🚀 Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server LocalDB (included with Visual Studio, or install separately)

### First-Time Setup

```powershell
# 1. Clone / navigate to the project
cd d:\Code\HotelBooking

# 2. Install EF Core tools (if not already installed)
dotnet tool install --global dotnet-ef

# 3. Add EF Core Design package (if not already added)
dotnet add HotelBooking.Web package Microsoft.EntityFrameworkCore.Design

# 4. Create the database migration
dotnet ef migrations add InitialCreate --project HotelBooking.Data --startup-project HotelBooking.Web

# 5. Run the application (auto-applies migrations + seeds data)
dotnet run --project HotelBooking.Web
```

### Subsequent Runs

```powershell
dotnet run --project HotelBooking.Web
```

The console will display:
```
Now listening on: http://localhost:5009
```
Open that URL in your browser.

---

## 🔑 Demo Accounts

The database **auto-seeds** on first run with sample data (20 rooms, 4 bookings, reviews, tickets).

**Password for all accounts:** `Hotel@123`

| Role | Email | Access |
|------|-------|--------|
| 👑 **Admin** | `admin@hotel.com` | Full access: Room CRUD, Booking management, Ticket management |
| 👷 **Staff** | `staff@hotel.com` | View bookings, Assign & resolve tickets |
| 🧳 **Customer** | `customer@hotel.com` | Browse rooms, Make bookings, Submit tickets, Leave reviews |

---

## 🔌 Database Configuration

The connection string is in `HotelBooking.Web/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=GrandAzureHotelDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

- Uses **SQL Server LocalDB** (no installation of full SQL Server needed)
- Database is created automatically on first run via EF Core Migrations
- Seed data populates roles, users, room types, rooms, bookings, reviews, and tickets

---

## 🧪 How to Test SignalR (Real-Time)

1. Open **Browser Window 1** → Login as `admin@hotel.com`  → Navigate to **Admin > Bookings**
2. Open **Browser Window 2** (Incognito) → Login as `customer@hotel.com` → Navigate to **Rooms** → Book a room
3. ⚡ Watch the Admin window instantly show a **toast notification** — no page refresh needed!
4. When Admin clicks **Confirm**, the customer's **My Bookings** page status updates in real-time

This works for **Bookings**, **Reviews**, and **Support Tickets**.

---

## 📋 All Page Routes

| Route | Auth Required | Role | Description |
|-------|:---:|------|-------------|
| `/` | ❌ | Any | Home page with hero section |
| `/Rooms` | ❌ | Any | Browse & filter rooms |
| `/Rooms/Detail?id=X` | ❌ | Any | Room details, reviews, AI concierge |
| `/Account/Login` | ❌ | Any | Login form |
| `/Account/Register` | ❌ | Any | Registration form |
| `/Account/Logout` | ✅ | Any | Logout action |
| `/Booking/Create?roomId=X` | ✅ | Customer | Create a booking |
| `/Booking/Confirmation?id=X` | ✅ | Customer | Booking confirmation |
| `/Booking/MyBookings` | ✅ | Customer | View & cancel bookings |
| `/Tickets` | ✅ | Customer/Staff | View tickets |
| `/Tickets/Create` | ✅ | Customer | Submit a support ticket |
| `/Admin/Rooms` | ✅ | Admin/Staff | Manage rooms |
| `/Admin/Rooms/Create` | ✅ | Admin/Staff | Add new room |
| `/Admin/Rooms/Edit?id=X` | ✅ | Admin/Staff | Edit existing room |
| `/Admin/Bookings` | ✅ | Admin/Staff | Confirm/complete bookings |
| `/Admin/Tickets` | ✅ | Admin/Staff | Assign & resolve tickets |

---

## 📄 License

This project is for educational purposes.
