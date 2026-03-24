using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace HotelBooking.Web.Pages.Admin.Bookings;

[Authorize(Roles = "Admin,Staff")]
public class IndexModel(IBookingService bookingService, IPaymentService paymentService, IHotelService hotelService) : PageModel
{
    public IReadOnlyList<BookingDto> Bookings { get; set; } = [];
    public string? Message { get; set; }
    public bool IsError { get; set; }
    [BindProperty] public string? RejectReason { get; set; }

    public async Task OnGetAsync()
    {
        var scoped = await GetScopedBookingsAsync();
        Bookings = scoped;
    }

    public async Task<IActionResult> OnPostApproveAsync(int bookingId)
    {
        if (!await CanAccessBookingAsync(bookingId)) return Forbid();
        var result = await bookingService.ApproveBookingAsync(bookingId);
        Message = result.IsSuccess ? "Booking approved. Awaiting guest payment." : result.ErrorMessage;
        IsError = !result.IsSuccess;
        await OnGetAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostRejectAsync(int bookingId)
    {
        if (!await CanAccessBookingAsync(bookingId)) return Forbid();
        var result = await bookingService.RejectBookingAsync(bookingId, RejectReason ?? "Rejected by hotel");
        Message = result.IsSuccess ? "Booking rejected." : result.ErrorMessage;
        IsError = !result.IsSuccess;
        await OnGetAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostCancelAsync(int bookingId)
    {
        var scoped = await GetScopedBookingsAsync();
        var booking = scoped.FirstOrDefault(b => b.Id == bookingId);
        if (booking is null) return Forbid();
        if (string.IsNullOrWhiteSpace(RejectReason))
        {
            Message = "Cancellation reason is required.";
            IsError = true;
            await OnGetAsync();
            return Page();
        }

        if (booking?.Status == "Confirmed")
        {
            var refundResult = await paymentService.RefundAsync(bookingId, reason: RejectReason.Trim());
            if (!refundResult.IsSuccess) { Message = refundResult.ErrorMessage; IsError = true; await OnGetAsync(); return Page(); }
        }
        var result = await bookingService.AdminCancelBookingAsync(bookingId, RejectReason.Trim());
        if (result.IsSuccess && booking?.Status == "Confirmed")
            Message = "Booking cancelled. Full refund processed.";
        else
            Message = result.IsSuccess ? "Booking cancelled." : result.ErrorMessage;
        IsError = !result.IsSuccess;
        await OnGetAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostCheckInAsync(int bookingId)
    {
        if (!await CanAccessBookingAsync(bookingId)) return Forbid();
        var result = await bookingService.CheckInAsync(bookingId);
        Message = result.IsSuccess ? "Guest checked in." : result.ErrorMessage;
        IsError = !result.IsSuccess;
        await OnGetAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostNoShowAsync(int bookingId)
    {
        if (!await CanAccessBookingAsync(bookingId)) return Forbid();
        var result = await bookingService.MarkNoShowAsync(bookingId);
        Message = result.IsSuccess ? "Booking marked as no-show." : result.ErrorMessage;
        IsError = !result.IsSuccess;
        await OnGetAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostCompleteAsync(int bookingId)
    {
        if (!await CanAccessBookingAsync(bookingId)) return Forbid();
        var result = await bookingService.CompleteBookingAsync(bookingId);
        if (result.IsSuccess)
        {
            var b = await bookingService.GetBookingByIdAsync(bookingId, userId: null);
            if (b.IsSuccess && b.Data?.Status == "AwaitingExtraPayment")
            {
                Message = "Check-out recorded. Awaiting guest payment for extra charges.";
                IsError = false;
            }
            else
            {
                Message = "Booking completed.";
                IsError = false;
            }
        }
        else
        {
            Message = result.ErrorMessage;
            IsError = true;
        }
        await OnGetAsync();
        return Page();
    }

    private async Task<IReadOnlyList<BookingDto>> GetScopedBookingsAsync()
    {
        var isAdmin = User.IsInRole("Admin");
        if (isAdmin)
        {
            var result = await bookingService.GetAllBookingsAsync();
            return result.IsSuccess ? result.Data! : [];
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return [];

        var hotelsResult = await hotelService.GetHotelsByStaffAsync(userId);
        if (!hotelsResult.IsSuccess || hotelsResult.Data is null) return [];

        var list = new List<BookingDto>();
        foreach (var h in hotelsResult.Data)
        {
            var b = await bookingService.GetBookingsByHotelAsync(h.Id);
            if (b.IsSuccess && b.Data is not null) list.AddRange(b.Data);
        }
        return list.OrderByDescending(x => x.CreatedAt).ToList();
    }

    private async Task<bool> CanAccessBookingAsync(int bookingId)
    {
        if (User.IsInRole("Admin")) return true;
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return false;

        var bookingResult = await bookingService.GetBookingByIdAsync(bookingId, userId: null);
        if (!bookingResult.IsSuccess || bookingResult.Data is null) return false;

        var hotelsResult = await hotelService.GetHotelsByStaffAsync(userId);
        if (!hotelsResult.IsSuccess || hotelsResult.Data is null) return false;

        var allowedHotelIds = hotelsResult.Data.Select(h => h.Id).ToHashSet();
        return allowedHotelIds.Contains(bookingResult.Data.HotelId);
    }
}
