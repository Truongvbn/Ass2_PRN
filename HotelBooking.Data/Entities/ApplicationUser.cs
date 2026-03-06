using Microsoft.AspNetCore.Identity;

namespace HotelBooking.Data.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<ReviewComment> ReviewComments { get; set; } = new List<ReviewComment>();
    public ICollection<SupportTicket> CreatedTickets { get; set; } = new List<SupportTicket>();
    public ICollection<SupportTicket> AssignedTickets { get; set; } = new List<SupportTicket>();
}
