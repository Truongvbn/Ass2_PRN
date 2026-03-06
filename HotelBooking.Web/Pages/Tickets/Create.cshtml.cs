using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using HotelBooking.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Web.Pages.Tickets;

[Authorize]
public class CreateModel(
    ITicketService ticketService,
    UserManager<ApplicationUser> userManager) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required] public string Category { get; set; } = "";
        [Required, StringLength(200, MinimumLength = 5)] public string Subject { get; set; } = "";
        [Required, StringLength(2000, MinimumLength = 10)] public string Description { get; set; } = "";
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var userId = userManager.GetUserId(User)!;
        var dto = new CreateTicketDto { Category = Input.Category, Subject = Input.Subject, Description = Input.Description };
        var result = await ticketService.CreateTicketAsync(dto, userId);

        if (result.IsSuccess)
            return RedirectToPage("/Tickets/Index");

        ErrorMessage = result.ErrorMessage;
        return Page();
    }
}
