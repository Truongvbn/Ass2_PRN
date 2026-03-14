namespace HotelBooking.Data.Entities;

public enum PaymentMethod
{
    CreditCard,
    DebitCard,
    BankTransfer,
    Cash
}

public enum PaymentStatus
{
    Pending,
    Completed,
    Failed,
    Refunded,
    PartialRefund
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

    public string? TransactionId { get; set; }
    public string? RefundReason { get; set; }
    public DateTime? RefundedAt { get; set; }
    public decimal RefundAmount { get; set; }

    // Navigation
    public Booking Booking { get; set; } = null!;
}
