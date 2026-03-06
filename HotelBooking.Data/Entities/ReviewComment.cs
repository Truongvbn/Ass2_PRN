namespace HotelBooking.Data.Entities;

public class ReviewComment
{
    public int Id { get; set; }
    public int ReviewId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Review Review { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
