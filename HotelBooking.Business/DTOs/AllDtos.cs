namespace HotelBooking.Business.DTOs;

// ── Room ──
public record RoomDto { public int Id { get; init; } public string Name { get; init; } = ""; public string RoomTypeName { get; init; } = ""; public decimal PricePerNight { get; init; } public int MaxOccupancy { get; init; } public string Description { get; init; } = ""; public string ImageUrl { get; init; } = ""; public string Amenities { get; init; } = ""; public bool IsAvailable { get; init; } public double AverageRating { get; init; } public int ReviewCount { get; init; } }
public record RoomListDto { public int Id { get; init; } public string Name { get; init; } = ""; public string RoomTypeName { get; init; } = ""; public decimal PricePerNight { get; init; } public int MaxOccupancy { get; init; } public string ImageUrl { get; init; } = ""; public bool IsAvailable { get; init; } public double AverageRating { get; init; } }
public record CreateRoomDto { public string Name { get; init; } = ""; public int RoomTypeId { get; init; } public decimal PricePerNight { get; init; } public int MaxOccupancy { get; init; } public string Description { get; init; } = ""; public string ImageUrl { get; init; } = ""; public string Amenities { get; init; } = ""; }
public record UpdateRoomDto { public int Id { get; init; } public string Name { get; init; } = ""; public int RoomTypeId { get; init; } public decimal PricePerNight { get; init; } public int MaxOccupancy { get; init; } public string Description { get; init; } = ""; public string ImageUrl { get; init; } = ""; public string Amenities { get; init; } = ""; public bool IsAvailable { get; init; } }

// ── RoomType ──
public record RoomTypeDto { public int Id { get; init; } public string Name { get; init; } = ""; public string Description { get; init; } = ""; }

// ── Booking ──
public record BookingDto { public int Id { get; init; } public int RoomId { get; init; } public string RoomName { get; init; } = ""; public string RoomTypeName { get; init; } = ""; public string UserId { get; init; } = ""; public string UserName { get; init; } = ""; public DateTime CheckIn { get; init; } public DateTime CheckOut { get; init; } public int NumberOfGuests { get; init; } public decimal TotalPrice { get; init; } public string Status { get; init; } = ""; public DateTime CreatedAt { get; init; } }
public record CreateBookingDto { public int RoomId { get; init; } public DateTime CheckIn { get; init; } public DateTime CheckOut { get; init; } public int NumberOfGuests { get; init; } }

// ── Payment ──
public record PaymentDto { public int Id { get; init; } public int BookingId { get; init; } public decimal Amount { get; init; } public string Method { get; init; } = ""; public string Status { get; init; } = ""; public DateTime? PaidAt { get; init; } }
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
