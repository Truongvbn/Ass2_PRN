namespace HotelBooking.Data.Entities;

public enum BookingStatus
{
    Pending,
    Confirmed,
    Completed,
    Cancelled
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

    // Navigation
    public Room Room { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    public Payment? Payment { get; set; }
}
