using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace HotelBooking.Web.Hubs;

[Authorize]
public class BookingHub : Hub
{
    private const string AdminGroup = "AdminGroup";

    public override async Task OnConnectedAsync()
    {
        if (Context.User?.IsInRole("Admin") == true || Context.User?.IsInRole("Staff") == true)
            await Groups.AddToGroupAsync(Context.ConnectionId, AdminGroup);
        await base.OnConnectedAsync();
    }
}

/// <summary>
/// SignalR hub for real-time review and comment updates.
/// </summary>
public class ReviewHub : Hub { }

/// <summary>
/// SignalR hub for real-time support ticket updates.
/// </summary>
public class TicketHub : Hub { }
