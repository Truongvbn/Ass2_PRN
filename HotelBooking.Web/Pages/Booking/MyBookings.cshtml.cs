using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using HotelBooking.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelBooking.Web.Pages.Booking;

[Authorize]
public class MyBookingsModel(
    IBookingService bookingService,
    UserManager<ApplicationUser> userManager) : PageModel
{
    public IReadOnlyList<BookingDto> Bookings { get; set; } = [];
    public string? Message { get; set; }
    public bool IsError { get; set; }

    public async Task OnGetAsync()
    {
        var userId = userManager.GetUserId(User)!;
        var result = await bookingService.GetUserBookingsAsync(userId);
        if (result.IsSuccess) Bookings = result.Data!;
    }

    public async Task<IActionResult> OnPostCancelAsync(int bookingId)
    {
        var userId = userManager.GetUserId(User)!;
        var result = await bookingService.CancelBookingAsync(bookingId, userId);
        Message = result.IsSuccess ? "Booking cancelled successfully." : result.ErrorMessage;
        IsError = !result.IsSuccess;

        var bookingsResult = await bookingService.GetUserBookingsAsync(userId);
        if (bookingsResult.IsSuccess) Bookings = bookingsResult.Data!;

        return Page();
    }
}
