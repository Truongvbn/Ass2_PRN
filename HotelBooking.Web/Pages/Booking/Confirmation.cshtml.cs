using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HotelBooking.Data.Entities;

namespace HotelBooking.Web.Pages.Booking;

[Authorize]
public class ConfirmationModel(
    IBookingService bookingService,
    UserManager<ApplicationUser> userManager) : PageModel
{
    public BookingDto? Booking { get; set; }

    public async Task OnGetAsync(int id)
    {
        var userId = User.IsInRole("Admin") || User.IsInRole("Staff") ? null : userManager.GetUserId(User);
        var result = await bookingService.GetBookingByIdAsync(id, userId);
        if (result.IsSuccess)
            Booking = result.Data;
    }
}
