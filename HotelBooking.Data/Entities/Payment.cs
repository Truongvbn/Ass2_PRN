namespace HotelBooking.Data.Entities;

public enum PaymentMethod
{
    Cash,
    Card,
    BankTransfer
}

public enum PaymentStatus
{
    Pending,
    Completed,
    Failed,
    Refunded
}

public class Payment
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Booking Booking { get; set; } = null!;
}
