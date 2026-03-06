using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelBooking.Web.Pages.Rooms;

public class IndexModel(IRoomService roomService) : PageModel
{
    public IReadOnlyList<RoomListDto> Rooms { get; set; } = [];
    public IReadOnlyList<RoomTypeDto> RoomTypes { get; set; } = [];

    [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
    public int? RoomTypeId { get; set; }

    [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
    public int? MinOccupancy { get; set; }

    [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
    public decimal? MaxPrice { get; set; }

    [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
    public DateTime? CheckIn { get; set; }

    [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
    public DateTime? CheckOut { get; set; }

    public async Task OnGetAsync()
    {
        RoomTypes = await roomService.GetRoomTypesAsync();
        var result = await roomService.SearchRoomsAsync(RoomTypeId, null, MaxPrice, MinOccupancy, CheckIn, CheckOut);
        if (result.IsSuccess)
            Rooms = result.Data!;
    }
}
