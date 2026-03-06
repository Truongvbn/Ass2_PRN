namespace HotelBooking.Data.Entities;

public class Review
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int Rating { get; set; } // 1-5
    public string Content { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Room Room { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    public ICollection<ReviewComment> Comments { get; set; } = new List<ReviewComment>();
}
