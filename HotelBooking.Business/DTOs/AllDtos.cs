namespace HotelBooking.Business.DTOs;

// ── Hotel ──
public record HotelDto
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string City { get; init; } = "";
    public string Address { get; init; } = "";
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public string PhoneNumber { get; init; } = "";
    public string Email { get; init; } = "";
    public string ImageUrl { get; init; } = "";
    public string[] Gallery { get; init; } = Array.Empty<string>();
    public int StarRating { get; init; }
    public bool IsActive { get; init; }
    public int RoomCount { get; init; }
    public decimal? MinPricePerNight { get; init; }
}

public record CreateHotelDto
{
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string Address { get; init; } = "";
    public string City { get; init; } = "";
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public string PhoneNumber { get; init; } = "";
    public string Email { get; init; } = "";
    public string ImageUrl { get; init; } = "";
    public int StarRating { get; init; } = 3;
}

public record UpdateHotelDto
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string Address { get; init; } = "";
    public string City { get; init; } = "";
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public string PhoneNumber { get; init; } = "";
    public string Email { get; init; } = "";
    public string ImageUrl { get; init; } = "";
    public int StarRating { get; init; } = 3;
    public bool IsActive { get; init; } = true;
}

public record HotelStaffDto
{
    public int Id { get; init; }
    public int HotelId { get; init; }
    public string HotelName { get; init; } = "";
    public string UserId { get; init; } = "";
    public string UserName { get; init; } = "";
    public string Role { get; init; } = "";
    public DateTime AssignedAt { get; init; }
}

public record AssignStaffDto
{
    public int HotelId { get; init; }
    public string UserId { get; init; } = "";
    public string Role { get; init; } = "";
}

// ── Room ──
public record RoomDto
{
    public int Id { get; init; }
    public int HotelId { get; init; }
    public string HotelName { get; init; } = "";
    public double? HotelLatitude { get; init; }
    public double? HotelLongitude { get; init; }
    public string Name { get; init; } = "";
    public int RoomTypeId { get; init; }
    public string RoomTypeName { get; init; } = "";
    public decimal PricePerNight { get; init; }
    public int MaxOccupancy { get; init; }
    public string Description { get; init; } = "";
    public string ImageUrl { get; init; } = "";
    public string[] Gallery { get; init; } = Array.Empty<string>();
    public string Amenities { get; init; } = "";
    public bool IsAvailable { get; init; }
    public double AverageRating { get; init; }
    public int ReviewCount { get; init; }
}

public record RoomListDto
{
    public int Id { get; init; }
    public int HotelId { get; init; }
    public string HotelName { get; init; } = "";
    public string Name { get; init; } = "";
    public string RoomTypeName { get; init; } = "";
    public decimal PricePerNight { get; init; }
    public int MaxOccupancy { get; init; }
    public string ImageUrl { get; init; } = "";
    public string[] Gallery { get; init; } = Array.Empty<string>();
    public bool IsAvailable { get; init; }
    public double AverageRating { get; init; }
}

public record CreateRoomDto
{
    public int HotelId { get; init; }
    public string Name { get; init; } = "";
    public int RoomTypeId { get; init; }
    public decimal PricePerNight { get; init; }
    public int MaxOccupancy { get; init; }
    public string Description { get; init; } = "";
    public string ImageUrl { get; init; } = "";
    public string[] Gallery { get; init; } = Array.Empty<string>();
    public string Amenities { get; init; } = "";
}

public record UpdateRoomDto
{
    public int Id { get; init; }
    public int HotelId { get; init; }
    public string Name { get; init; } = "";
    public int RoomTypeId { get; init; }
    public decimal PricePerNight { get; init; }
    public int MaxOccupancy { get; init; }
    public string Description { get; init; } = "";
    public string ImageUrl { get; init; } = "";
    public string[] Gallery { get; init; } = Array.Empty<string>();
    public string Amenities { get; init; } = "";
    public bool IsAvailable { get; init; }
}

// ── RoomType ──
public record RoomTypeDto { public int Id { get; init; } public string Name { get; init; } = ""; public string Description { get; init; } = ""; }

// ── Booking ──
public record BookingDto { public int Id { get; init; } public int HotelId { get; init; } public string HotelName { get; init; } = ""; public int RoomId { get; init; } public string RoomName { get; init; } = ""; public string RoomTypeName { get; init; } = ""; public string UserId { get; init; } = ""; public string UserName { get; init; } = ""; public DateTime CheckIn { get; init; } public DateTime CheckOut { get; init; } public int NumberOfGuests { get; init; } public decimal TotalPrice { get; init; } public string Status { get; init; } = ""; public DateTime CreatedAt { get; init; } public string? GuestNotes { get; init; } public string? CancellationReason { get; init; } public DateTime? ConfirmedAt { get; init; } public DateTime? PaymentDeadline { get; init; } public DateTime? CheckedInAt { get; init; } public DateTime? CheckedOutAt { get; init; } public DateTime? CancelledAt { get; init; } public decimal? RefundAmount { get; init; } public DateTime? RefundedAt { get; init; } public decimal ExtraChargeAmount { get; init; } public string? ExtraChargeDescription { get; init; } public bool IsExtraChargePaid { get; init; } public string? LostAndFoundNotes { get; init; } public string? LostAndFoundImageUrl { get; init; } }
public record CreateBookingDto { public int RoomId { get; init; } public DateTime CheckIn { get; init; } public DateTime CheckOut { get; init; } public int NumberOfGuests { get; init; } public string? GuestNotes { get; init; } }
public record CheckoutBookingDto { public decimal ExtraChargeAmount { get; init; } public string? ExtraChargeDescription { get; init; } public string? LostAndFoundNotes { get; init; } public string? LostAndFoundImageUrl { get; init; } }

// ── Payment ──
public record PaymentDto { public int Id { get; init; } public int BookingId { get; init; } public decimal Amount { get; init; } public string Method { get; init; } = ""; public string Status { get; init; } = ""; public DateTime? PaidAt { get; init; } public decimal RefundAmount { get; init; } public DateTime? RefundedAt { get; init; } }
public record CreatePaymentDto { public int BookingId { get; init; } public string Method { get; init; } = ""; }

// ── Review ──
public record ReviewDto { public int Id { get; init; } public int RoomId { get; init; } public string UserName { get; init; } = ""; public int Rating { get; init; } public string Content { get; init; } = ""; public DateTime CreatedAt { get; init; } public List<ReviewCommentDto> Comments { get; init; } = new(); }
public record CreateReviewDto { public int RoomId { get; init; } public int Rating { get; init; } public string Content { get; init; } = ""; }
public record UpdateReviewDto { public int Id { get; init; } public int Rating { get; init; } public string Content { get; init; } = ""; }

// ── ReviewComment ──
public record ReviewCommentDto { public int Id { get; init; } public int ReviewId { get; init; } public string UserName { get; init; } = ""; public string Content { get; init; } = ""; public DateTime CreatedAt { get; init; } }
public record CreateReviewCommentDto { public int ReviewId { get; init; } public string Content { get; init; } = ""; }

// ── SupportTicket ──
public record TicketDto { public int Id { get; init; } public string UserId { get; init; } = ""; public string UserName { get; init; } = ""; public string? AssignedToName { get; init; } public string Category { get; init; } = ""; public string Priority { get; init; } = ""; public string Subject { get; init; } = ""; public string Description { get; init; } = ""; public string Status { get; init; } = ""; public DateTime CreatedAt { get; init; } public DateTime? ClosedAt { get; init; } }
public record CreateTicketDto { public string Category { get; init; } = ""; public string Subject { get; init; } = ""; public string Description { get; init; } = ""; }
public record UpdateTicketStatusDto { public int Id { get; init; } public string Status { get; init; } = ""; }

// ── AI ──
public record RoomPreferenceDto { public decimal? MaxBudget { get; init; } public int? GuestCount { get; init; } public List<string>? DesiredAmenities { get; init; } public DateTime? CheckIn { get; init; } public DateTime? CheckOut { get; init; } }

// ── Reporting ──
public record FinancialReportDto
{
    public decimal TotalRevenue { get; init; }
    public decimal BookingRevenue { get; init; }
    public decimal ExtraChargeRevenue { get; init; }
    public decimal RefundAmount { get; init; }
    public int TotalBookings { get; init; }
    public int CompletedBookings { get; init; }
    public int CancelledBookings { get; init; }
    public List<DailyRevenueDto> DailyRevenues { get; init; } = new();
    public List<HotelFinancialReportDto> HotelReports { get; init; } = new();
}

public record DailyRevenueDto
{
    public DateTime Date { get; init; }
    public decimal Revenue { get; init; }
    public int BookingCount { get; init; }
}

public record HotelFinancialReportDto
{
    public int HotelId { get; init; }
    public string HotelName { get; init; } = "";
    public decimal TotalRevenue { get; init; }
    public int BookingCount { get; init; }
}
