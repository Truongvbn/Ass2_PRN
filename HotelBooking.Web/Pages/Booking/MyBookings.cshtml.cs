using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelBooking.Web.Pages.Booking;

[Authorize]
public class MyBookingsModel(
    IBookingService bookingService,
    IPaymentService paymentService) : PageModel
{
    public IReadOnlyList<BookingDto> Bookings { get; set; } = [];
    public string? Message { get; set; }
    public bool IsError { get; set; }

    public async Task OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await bookingService.GetUserBookingsAsync(userId);
        if (result.IsSuccess) Bookings = result.Data!;
    }

    public async Task<IActionResult> OnPostCancelAsync(int bookingId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var bookingsResult = await bookingService.GetUserBookingsAsync(userId);
        var booking = bookingsResult.Data?.FirstOrDefault(b => b.Id == bookingId);
        if (booking?.Status == "Confirmed")
        {
            var refundResult = await paymentService.RefundAsync(bookingId, reason: "Cancelled by guest");
            if (!refundResult.IsSuccess) { Message = refundResult.ErrorMessage; IsError = true; Bookings = bookingsResult.Data ?? []; return Page(); }
        }
        var result = await bookingService.CancelBookingAsync(bookingId, userId);
        Message = result.IsSuccess ? "Booking cancelled successfully." : result.ErrorMessage;
        IsError = !result.IsSuccess;

        bookingsResult = await bookingService.GetUserBookingsAsync(userId);
        if (bookingsResult.IsSuccess) Bookings = bookingsResult.Data!;

        return Page();
    }
}
