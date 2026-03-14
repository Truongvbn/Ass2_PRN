using HotelBooking.Business.DTOs;

namespace HotelBooking.Business.Services.Interfaces;

public interface IBookingHubNotifier
{
    Task NewBookingRequest(BookingDto booking);
    Task BookingApproved(int bookingId, string userId);
    Task BookingRejected(int bookingId, string userId, string reason);
    Task PaymentReceived(int bookingId);
    Task BookingConfirmed(int bookingId, string userId);
    Task RefundProcessed(int bookingId, string userId, decimal amount);
    Task BookingExpired(int bookingId, string userId);
    Task CheckedIn(int bookingId, string userId);
    Task StayCompleted(int bookingId, string userId);
    Task NoShow(int bookingId);
    Task BookingCreated(BookingDto booking);
    Task BookingStatusChanged(int bookingId, string newStatus);
    Task BookingCancelled(int bookingId);
}

public interface IReviewHubNotifier
{
    Task ReviewCreated(ReviewDto review);
    Task ReviewUpdated(ReviewDto review);
    Task ReviewDeleted(int reviewId, int roomId);
    Task CommentAdded(ReviewCommentDto comment);
    Task CommentDeleted(int commentId, int reviewId);
}

public interface ITicketHubNotifier
{
    Task TicketCreated(TicketDto ticket);
    Task TicketAssigned(int ticketId, string staffName);
    Task TicketStatusChanged(int ticketId, string newStatus);
    Task TicketClosed(int ticketId);
}
