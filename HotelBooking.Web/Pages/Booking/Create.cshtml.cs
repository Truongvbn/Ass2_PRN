using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Web.Pages.Booking;

[Authorize]
public class CreateModel(
    IRoomService roomService,
    IBookingService bookingService) : PageModel
{
    public RoomDto? Room { get; set; }
    public string? ErrorMessage { get; set; }

    [BindProperty] public InputModel Input { get; set; } = new();

    public int RoomId => Input.RoomId;

    public class InputModel
    {
        public int RoomId { get; set; }

        [Required, DataType(DataType.Date)]
        [Display(Name = "Check-In Date")]
        public DateTime CheckIn { get; set; } = DateTime.UtcNow.Date;

        [Required, DataType(DataType.Date)]
        [Display(Name = "Check-Out Date")]
        public DateTime CheckOut { get; set; } = DateTime.UtcNow.Date.AddDays(1);

        [Required, Range(1, 20)]
        [Display(Name = "Number of Guests")]
        public int NumberOfGuests { get; set; } = 1;

        [Display(Name = "Special Requests")]
        public string? GuestNotes { get; set; }
    }

    public async Task OnGetAsync(int roomId)
    {
        Input.RoomId = roomId;
        var result = await roomService.GetRoomByIdAsync(roomId);
        if (result.IsSuccess) Room = result.Data;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var roomResult = await roomService.GetRoomByIdAsync(Input.RoomId);
        if (roomResult.IsSuccess) Room = roomResult.Data;

        if (!ModelState.IsValid) return Page();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var dto = new CreateBookingDto { RoomId = Input.RoomId, CheckIn = Input.CheckIn, CheckOut = Input.CheckOut, NumberOfGuests = Input.NumberOfGuests, GuestNotes = Input.GuestNotes };
        var result = await bookingService.CreateBookingAsync(dto, userId);

        if (result.IsSuccess)
            return RedirectToPage("/Booking/Confirmation", new { id = result.Data!.Id });

        ErrorMessage = result.ErrorMessage;
        return Page();
    }
}
