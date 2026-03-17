using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelBooking.Web.Pages.Admin.Bookings;

[Authorize(Roles = "Admin,Staff")]
public class DetailModel(IBookingService bookingService, IPaymentService paymentService) : PageModel
{
    public BookingDto? Booking { get; set; }
    public string? ErrorMessage { get; set; }
    [BindProperty] public string? RejectReason { get; set; }

    public async Task<IActionResult> OnGetAsync(int id, string? error)
    {
        if (!string.IsNullOrEmpty(error)) ErrorMessage = error;
        var result = await bookingService.GetBookingByIdAsync(id);
        if (!result.IsSuccess)
        {
            ErrorMessage ??= result.ErrorMessage ?? "Booking not found.";
            return Page();
        }
        Booking = result.Data;
        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync(int id)
    {
        var result = await bookingService.ApproveBookingAsync(id);
        return result.IsSuccess ? RedirectToPage(new { id }) : RedirectToPage(new { id, error = result.ErrorMessage });
    }

    public async Task<IActionResult> OnPostRejectAsync(int id)
    {
        var result = await bookingService.RejectBookingAsync(id, RejectReason ?? "Rejected");
        return result.IsSuccess ? RedirectToPage(new { id }) : RedirectToPage(new { id, error = result.ErrorMessage });
    }

    public async Task<IActionResult> OnPostCheckInAsync(int id)
    {
        var result = await bookingService.CheckInAsync(id);
        return result.IsSuccess ? RedirectToPage(new { id }) : RedirectToPage(new { id, error = result.ErrorMessage });
    }

    public async Task<IActionResult> OnPostNoShowAsync(int id)
    {
        var result = await bookingService.MarkNoShowAsync(id);
        return result.IsSuccess ? RedirectToPage(new { id }) : RedirectToPage(new { id, error = result.ErrorMessage });
    }

    [BindProperty] public CheckoutBookingDto CheckoutDto { get; set; } = new();

    public async Task<IActionResult> OnPostCompleteAsync(int id)
    {
        var result = await bookingService.CompleteBookingAsync(id, CheckoutDto);
        return result.IsSuccess ? RedirectToPage(new { id }) : RedirectToPage(new { id, error = result.ErrorMessage });
    }

    public async Task<IActionResult> OnPostCancelAsync(int id)
    {
        var bResult = await bookingService.GetBookingByIdAsync(id);
        if (bResult.IsSuccess && bResult.Data?.Status == "Confirmed")
        {
            var refundResult = await paymentService.RefundAsync(id, reason: RejectReason ?? "Cancelled by admin");
            if (!refundResult.IsSuccess) return RedirectToPage(new { id, error = refundResult.ErrorMessage });
        }
        var result = await bookingService.AdminCancelBookingAsync(id, RejectReason);
        return result.IsSuccess ? RedirectToPage(new { id }) : RedirectToPage(new { id, error = result.ErrorMessage });
    }
}
