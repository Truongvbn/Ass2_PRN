namespace HotelBooking.Data.Entities;

public enum BookingStatus
{
    Pending,         // Guest submitted, hotel hasn't reviewed yet
    AwaitingPayment, // Hotel approved, waiting for guest to pay
    Confirmed,       // Paid and locked in
    CheckedIn,       // Guest physically checked in
    Completed,       // Stay finished, checked out
    Cancelled,       // Cancelled by guest or admin
    Rejected,        // Hotel declined the request
    Expired,         // Timed out (no hotel response or no payment)
    NoShow           // Guest didn't show up on check-in day
}

public class Booking
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public int NumberOfGuests { get; set; }
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public byte[] RowVersion { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Lifecycle metadata
    public string? GuestNotes { get; set; }
    public string? AdminNotes { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? PaymentDeadline { get; set; }
    public DateTime? CheckedInAt { get; set; }
    public DateTime? CheckedOutAt { get; set; }

    // Navigation
    public Room Room { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    public Payment? Payment { get; set; }
}
