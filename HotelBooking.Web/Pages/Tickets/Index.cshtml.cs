using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using HotelBooking.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelBooking.Web.Pages.Tickets;

[Authorize]
public class IndexModel(
    ITicketService ticketService,
    UserManager<ApplicationUser> userManager) : PageModel
{
    public IReadOnlyList<TicketDto> Tickets { get; set; } = [];
    public string? Message { get; set; }
    public bool IsError { get; set; }

    public async Task OnGetAsync()
    {
        var userId = userManager.GetUserId(User)!;
        var isStaff = User.IsInRole("Admin") || User.IsInRole("Staff");

        var result = isStaff
            ? await ticketService.GetActiveTicketsAsync()
            : await ticketService.GetUserTicketsAsync(userId);

        if (result.IsSuccess) Tickets = result.Data!;
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(int ticketId, string newStatus)
    {
        var userId = userManager.GetUserId(User)!;
        var isStaff = User.IsInRole("Admin") || User.IsInRole("Staff");
        var result = await ticketService.UpdateTicketStatusAsync(ticketId, newStatus, userId, isStaff);
        Message = result.IsSuccess ? $"Ticket status updated to {newStatus}." : result.ErrorMessage;
        IsError = !result.IsSuccess;
        await OnGetAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostReopenAsync(int ticketId)
    {
        var userId = userManager.GetUserId(User)!;
        var result = await ticketService.UpdateTicketStatusAsync(ticketId, "Open", userId, false);
        Message = result.IsSuccess ? "Ticket reopened." : result.ErrorMessage;
        IsError = !result.IsSuccess;
        await OnGetAsync();
        return Page();
    }
}
