using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace HotelBooking.Web.Pages.Booking;

[Authorize]
public class PaymentModel(
    IBookingService bookingService,
    IPaymentService paymentService) : PageModel
{
    public BookingDto? Booking { get; set; }
    public string? ErrorMessage { get; set; }

    [BindProperty] public int BookingId { get; set; }
    [BindProperty] public string Method { get; set; } = "CreditCard";

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await bookingService.GetBookingByIdAsync(id, userId);

        if (!result.IsSuccess)
            return RedirectToPage("/Booking/MyBookings");

        Booking = result.Data;
        BookingId = id;

        if (Booking!.Status != "AwaitingPayment")
            return RedirectToPage("/Booking/Confirmation", new { id });

        if (Booking.PaymentDeadline.HasValue && Booking.PaymentDeadline.Value < DateTime.UtcNow)
        {
            TempData["ErrorMessage"] = "Payment window has expired.";
            return RedirectToPage("/Booking/Confirmation", new { id });
        }

        // Check if already paid
        var paymentResult = await paymentService.GetPaymentByBookingAsync(id);
        if (paymentResult.IsSuccess)
            return RedirectToPage("/Booking/Confirmation", new { id });

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var bookingResult = await bookingService.GetBookingByIdAsync(BookingId, userId);

        if (!bookingResult.IsSuccess)
            return RedirectToPage("/Booking/MyBookings");

        var dto = new CreatePaymentDto { BookingId = BookingId, Method = Method };
        var result = await paymentService.ProcessPaymentAsync(dto);

        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Payment processed successfully!";
            return RedirectToPage("/Booking/Confirmation", new { id = BookingId });
        }

        ErrorMessage = result.ErrorMessage;
        Booking = bookingResult.Data;
        return Page();
    }
}
