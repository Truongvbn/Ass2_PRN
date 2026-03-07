using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelBooking.Web.Pages.Admin.Bookings;

[Authorize(Roles = "Admin,Staff")]
public class DetailModel(IBookingService bookingService) : PageModel
{
    public BookingDto? Booking { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await bookingService.GetBookingByIdAsync(id);
        if (!result.IsSuccess)
        {
            ErrorMessage = result.ErrorMessage ?? "Booking not found.";
            return Page();
        }

        Booking = result.Data;
        return Page();
    }
}
