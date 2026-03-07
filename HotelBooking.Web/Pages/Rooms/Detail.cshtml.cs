using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelBooking.Web.Pages.Rooms;

public class DetailModel(
    IRoomService roomService,
    IReviewService reviewService,
    IBookingService bookingService) : PageModel
{
    public RoomDto? Room { get; set; }
    public IReadOnlyList<ReviewDto> Reviews { get; set; } = [];
    public bool CanReview { get; set; }
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(int id)
    {
        if (TempData.ContainsKey("Error"))
            ErrorMessage = TempData["Error"]?.ToString();
        if (TempData.ContainsKey("Success"))
            SuccessMessage = TempData["Success"]?.ToString();

        var roomResult = await roomService.GetRoomByIdAsync(id);
        if (!roomResult.IsSuccess) return;
        Room = roomResult.Data;

        var reviewResult = await reviewService.GetRoomReviewsAsync(id);
        if (reviewResult.IsSuccess) Reviews = reviewResult.Data!;

        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var bookingsResult = await bookingService.GetUserBookingsAsync(userId);
            CanReview = bookingsResult.IsSuccess &&
                        bookingsResult.Data!.Any(b => b.RoomId == id && b.Status == "Completed");
        }
    }

    public async Task<IActionResult> OnPostCreateReviewAsync(int roomId, int rating, string content)
    {
        if (!User.Identity!.IsAuthenticated)
            return RedirectToPage("/Account/Login");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await reviewService.CreateReviewAsync(
            new CreateReviewDto { RoomId = roomId, Rating = rating, Content = content }, userId);

        if (!result.IsSuccess)
            TempData["Error"] = result.ErrorMessage;

        return RedirectToPage(new { id = roomId });
    }

    public async Task<IActionResult> OnPostAddCommentAsync(int reviewId, string content)
    {
        if (!User.Identity!.IsAuthenticated)
            return RedirectToPage("/Account/Login");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await reviewService.AddCommentAsync(new CreateReviewCommentDto { ReviewId = reviewId, Content = content }, userId);
        TempData["Success"] = "Comment added successfully.";

        // Get roomId for redirect — find which room the review belongs to
        return RedirectToPage(new { id = GetRoomId() });
    }

    private string GetRoomId() => Request.Query["id"].FirstOrDefault() ?? "0";

    public async Task<IActionResult> OnPostUpdateReviewAsync(int reviewId, int rating, string content)
    {
        if (!User.Identity!.IsAuthenticated)
            return RedirectToPage("/Account/Login");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await reviewService.UpdateReviewAsync(
            new UpdateReviewDto { Id = reviewId, Rating = rating, Content = content }, userId);

        if (!result.IsSuccess)
            TempData["Error"] = result.ErrorMessage;
        else
            TempData["Success"] = "Review updated successfully.";

        return RedirectToPage(new { id = GetRoomId() });
    }

    public async Task<IActionResult> OnPostDeleteReviewAsync(int reviewId)
    {
        if (!User.Identity!.IsAuthenticated)
            return RedirectToPage("/Account/Login");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isAdmin = User.IsInRole("Admin") || User.IsInRole("Staff");
        var result = await reviewService.DeleteReviewAsync(reviewId, userId, isAdmin);

        if (!result.IsSuccess)
            TempData["Error"] = result.ErrorMessage;

        return RedirectToPage(new { id = GetRoomId() });
    }

    public async Task<IActionResult> OnPostDeleteCommentAsync(int commentId)
    {
        if (!User.Identity!.IsAuthenticated)
            return RedirectToPage("/Account/Login");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isAdmin = User.IsInRole("Admin") || User.IsInRole("Staff");
        var result = await reviewService.DeleteCommentAsync(commentId, userId, isAdmin);

        if (!result.IsSuccess)
            TempData["Error"] = result.ErrorMessage;

        return RedirectToPage(new { id = GetRoomId() });
    }
}
