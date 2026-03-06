namespace HotelBooking.Data.Entities;

public enum TicketCategory
{
    Room,
    Service,
    Payment,
    Other
}

public enum TicketPriority
{
    Low,
    Medium,
    High,
    Urgent
}

public enum TicketStatus
{
    Open,
    InProgress,
    Resolved,
    Closed
}

public class SupportTicket
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? AssignedToId { get; set; }
    public TicketCategory Category { get; set; }
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }

    // Navigation
    public ApplicationUser User { get; set; } = null!;
    public ApplicationUser? AssignedTo { get; set; }
}
