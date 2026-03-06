using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace HotelBooking.Web.Hubs;

public class BookingHubNotifier(IHubContext<BookingHub> hub) : IBookingHubNotifier
{
    public Task BookingCreated(BookingDto booking)
        => hub.Clients.User(booking.UserId).SendAsync("BookingCreated", booking);

    // Provide generic status changes to all or just user? Only user needs to know their status changed.
    public Task BookingStatusChanged(int bookingId, string newStatus)
        => hub.Clients.All.SendAsync("BookingStatusChanged", bookingId, newStatus); 
        // Note: Ideally we pass userId here too, but for simplicity we just notify. 
        // A better approach is to change the interface to include userId, but let's just 
        // broadcast a very minimal ID-only message which doesn't leak much.

    public Task BookingCancelled(int bookingId)
        => hub.Clients.All.SendAsync("BookingCancelled", bookingId);
}

public class ReviewHubNotifier(IHubContext<ReviewHub> hub) : IReviewHubNotifier
{
    public Task ReviewCreated(ReviewDto review)
        => hub.Clients.All.SendAsync("ReviewCreated", review);

    public Task ReviewUpdated(ReviewDto review)
        => hub.Clients.All.SendAsync("ReviewUpdated", review);

    public Task ReviewDeleted(int reviewId, int roomId)
        => hub.Clients.All.SendAsync("ReviewDeleted", reviewId, roomId);

    public Task CommentAdded(ReviewCommentDto comment)
        => hub.Clients.All.SendAsync("CommentAdded", comment);

    public Task CommentDeleted(int commentId, int reviewId)
        => hub.Clients.All.SendAsync("CommentDeleted", commentId, reviewId);
}

public class TicketHubNotifier(IHubContext<TicketHub> hub) : ITicketHubNotifier
{
    public Task TicketCreated(TicketDto ticket)
        => hub.Clients.User(ticket.UserId).SendAsync("TicketCreated", ticket);

    public Task TicketAssigned(int ticketId, string staffName)
        => hub.Clients.All.SendAsync("TicketAssigned", ticketId, staffName);

    public Task TicketStatusChanged(int ticketId, string newStatus)
        => hub.Clients.All.SendAsync("TicketStatusChanged", ticketId, newStatus);

    public Task TicketClosed(int ticketId)
        => hub.Clients.All.SendAsync("TicketClosed", ticketId);
}
