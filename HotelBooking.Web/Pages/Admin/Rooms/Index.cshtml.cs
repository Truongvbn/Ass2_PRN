using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelBooking.Web.Pages.Admin.Rooms;

[Authorize(Roles = "Admin,Staff")]
public class IndexModel(IRoomService roomService) : PageModel
{
    public IReadOnlyList<RoomListDto> Rooms { get; set; } = [];
    public string? Message { get; set; }
    public bool IsError { get; set; }

    public async Task OnGetAsync()
    {
        var result = await roomService.SearchRoomsAsync(null, null, null, null, null, null);
        if (result.IsSuccess) Rooms = result.Data!;
    }

    public async Task<IActionResult> OnPostDeleteAsync(int roomId)
    {
        var result = await roomService.DeleteRoomAsync(roomId);
        Message = result.IsSuccess ? "Room deleted successfully." : result.ErrorMessage;
        IsError = !result.IsSuccess;
        await OnGetAsync();
        return Page();
    }
}
