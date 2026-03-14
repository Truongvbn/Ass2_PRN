using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelBooking.Web.Pages.Admin.Hotels;

[Authorize(Roles = "Admin")]
public class IndexModel(IHotelService hotelService) : PageModel
{
    public IReadOnlyList<HotelDto> Hotels { get; set; } = [];

    public async Task OnGetAsync()
    {
        var result = await hotelService.GetAllHotelsAsync();
        if (result.IsSuccess && result.Data is not null) Hotels = result.Data;
    }
}

