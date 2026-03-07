using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelBooking.Web.Pages.Admin.Tickets;

[Authorize(Roles = "Admin,Staff")]
public class IndexModel(
    ITicketService ticketService) : PageModel
{
    public IReadOnlyList<TicketDto> Tickets { get; set; } = [];
    public string? Message { get; set; }
    public bool IsError { get; set; }

    public async Task OnGetAsync()
    {
        var result = await ticketService.GetActiveTicketsAsync();
        if (result.IsSuccess) Tickets = result.Data!;
    }

    public async Task<IActionResult> OnPostAssignAsync(int ticketId)
    {
        var staffId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await ticketService.AssignTicketAsync(ticketId, staffId);
        Message = result.IsSuccess ? "Ticket assigned to you." : result.ErrorMessage;
        IsError = !result.IsSuccess;
        await OnGetAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(int ticketId, string newStatus)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await ticketService.UpdateTicketStatusAsync(ticketId, newStatus, userId, isStaff: true);
        Message = result.IsSuccess ? $"Ticket status updated to {newStatus}." : result.ErrorMessage;
        IsError = !result.IsSuccess;
        await OnGetAsync();
        return Page();
    }
}
