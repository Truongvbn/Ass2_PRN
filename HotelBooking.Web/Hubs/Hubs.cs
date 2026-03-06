using Microsoft.AspNetCore.SignalR;

namespace HotelBooking.Web.Hubs;

/// <summary>
/// SignalR hub for real-time booking updates.
/// This is a marker class — all broadcasting is done via IHubContext in services.
/// </summary>
public class BookingHub : Hub { }

/// <summary>
/// SignalR hub for real-time review and comment updates.
/// </summary>
public class ReviewHub : Hub { }

/// <summary>
/// SignalR hub for real-time support ticket updates.
/// </summary>
public class TicketHub : Hub { }
