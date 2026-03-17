using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.SignalR; 

namespace HotelBooking.Web.Hubs;

public class BookingHubNotifier(IHubContext<BookingHub> hub) : IBookingHubNotifier
{
    private const string AdminGroup = "AdminGroup";

    public Task NewBookingRequest(BookingDto booking)
        => hub.Clients.Group(AdminGroup).SendAsync("NewBookingRequest", booking);

    public Task BookingApproved(int bookingId, string userId)
        => hub.Clients.User(userId).SendAsync("BookingApproved", bookingId);

    public Task BookingRejected(int bookingId, string userId, string reason)
        => hub.Clients.User(userId).SendAsync("BookingRejected", bookingId, reason);

    public Task PaymentReceived(int bookingId)
        => hub.Clients.Group(AdminGroup).SendAsync("PaymentReceived", bookingId);

    public Task BookingConfirmed(int bookingId, string userId)
        => hub.Clients.User(userId).SendAsync("BookingConfirmed", bookingId);

    public Task RefundProcessed(int bookingId, string userId, decimal amount)
        => hub.Clients.User(userId).SendAsync("RefundProcessed", bookingId, amount);

    public Task BookingExpired(int bookingId, string userId)
        => hub.Clients.User(userId).SendAsync("BookingExpired", bookingId);

    public Task CheckedIn(int bookingId, string userId)
        => hub.Clients.User(userId).SendAsync("CheckedIn", bookingId);

    public Task StayCompleted(int bookingId, string userId)
        => hub.Clients.User(userId).SendAsync("StayCompleted", bookingId);

    public Task NoShow(int bookingId)
        => hub.Clients.Group(AdminGroup).SendAsync("NoShow", bookingId);

    public Task BookingCreated(BookingDto booking)
        => hub.Clients.User(booking.UserId).SendAsync("BookingCreated", booking);

    public Task BookingStatusChanged(int bookingId, string newStatus)
        => hub.Clients.All.SendAsync("BookingStatusChanged", bookingId, newStatus);

    public Task BookingCancelled(int bookingId)
        => hub.Clients.All.SendAsync("BookingCancelled", bookingId);

    public Task RoomLocked(int roomId, string roomName)
        => hub.Clients.All.SendAsync("RoomLocked", roomId, roomName);
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
        => hub.Clients.All.SendAsync("TicketCreated", ticket);

    public Task TicketAssigned(int ticketId, string staffName)
        => hub.Clients.All.SendAsync("TicketAssigned", ticketId, staffName);

    public Task TicketStatusChanged(int ticketId, string newStatus)
        => hub.Clients.All.SendAsync("TicketStatusChanged", ticketId, newStatus);

    public Task TicketClosed(int ticketId)
        => hub.Clients.All.SendAsync("TicketClosed", ticketId);
}
