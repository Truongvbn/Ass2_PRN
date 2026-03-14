using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace HotelBooking.Web.Pages.Admin.Rooms;

[Authorize(Roles = "Admin,Staff")]
public class IndexModel(IRoomService roomService, IHotelService hotelService) : PageModel
{
    public IReadOnlyList<RoomListDto> Rooms { get; set; } = [];
    public string? Message { get; set; }
    public bool IsError { get; set; }

    public async Task OnGetAsync()
    {
        var isAdmin = User.IsInRole("Admin");
        if (isAdmin)
        {
            var result = await roomService.SearchRoomsAsync(null, null, null, null, null, null, null);
            if (result.IsSuccess) Rooms = result.Data!;
            return;
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return;

        var hotels = await hotelService.GetHotelsByStaffAsync(userId);
        if (!hotels.IsSuccess || hotels.Data is null) return;

        var list = new List<RoomListDto>();
        foreach (var h in hotels.Data)
        {
            var r = await roomService.SearchRoomsAsync(h.Id, null, null, null, null, null, null);
            if (r.IsSuccess && r.Data is not null) list.AddRange(r.Data);
        }

        Rooms = list.OrderBy(r => r.PricePerNight).ToList();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int roomId)
    {
        if (!await CanAccessRoomAsync(roomId)) return Forbid();
        var result = await roomService.DeleteRoomAsync(roomId);
        Message = result.IsSuccess ? "Room deleted successfully." : result.ErrorMessage;
        IsError = !result.IsSuccess;
        await OnGetAsync();
        return Page();
    }

    private async Task<bool> CanAccessRoomAsync(int roomId)
    {
        if (User.IsInRole("Admin")) return true;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return false;

        var roomResult = await roomService.GetRoomByIdAsync(roomId);
        if (!roomResult.IsSuccess || roomResult.Data is null) return false;

        var hotelsResult = await hotelService.GetHotelsByStaffAsync(userId);
        if (!hotelsResult.IsSuccess || hotelsResult.Data is null) return false;

        var allowedHotelIds = hotelsResult.Data.Select(h => h.Id).ToHashSet();
        return allowedHotelIds.Contains(roomResult.Data.HotelId);
    }
}
