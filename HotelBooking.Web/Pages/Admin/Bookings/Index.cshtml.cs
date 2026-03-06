using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelBooking.Web.Pages.Admin.Bookings;

[Authorize(Roles = "Admin,Staff")]
public class IndexModel(IBookingService bookingService) : PageModel
{
    public IReadOnlyList<BookingDto> Bookings { get; set; } = [];
    public string? Message { get; set; }
    public bool IsError { get; set; }

    public async Task OnGetAsync()
    {
        var result = await bookingService.GetAllBookingsAsync();
        if (result.IsSuccess) Bookings = result.Data!;
    }

    public async Task<IActionResult> OnPostConfirmAsync(int bookingId)
    {
        var result = await bookingService.ConfirmBookingAsync(bookingId);
        Message = result.IsSuccess ? "Booking confirmed." : result.ErrorMessage;
        IsError = !result.IsSuccess;
        await OnGetAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostCompleteAsync(int bookingId)
    {
        var result = await bookingService.CompleteBookingAsync(bookingId);
        Message = result.IsSuccess ? "Booking completed." : result.ErrorMessage;
        IsError = !result.IsSuccess;
        await OnGetAsync();
        return Page();
    }
}
