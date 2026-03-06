using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Web.Pages.Admin.Rooms;

[Authorize(Roles = "Admin,Staff")]
public class CreateModel(IRoomService roomService) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();
    public IReadOnlyList<RoomTypeDto> RoomTypes { get; set; } = [];
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required] public string Name { get; set; } = "";
        [Required] public int RoomTypeId { get; set; }
        [Required, Range(1, 10000)] public decimal PricePerNight { get; set; }
        [Required, Range(1, 50)] public int MaxOccupancy { get; set; }
        public string ImageUrl { get; set; } = "";
        public string Amenities { get; set; } = "";
        public string Description { get; set; } = "";
    }

    public async Task OnGetAsync()
    {
        RoomTypes = await roomService.GetRoomTypesAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        RoomTypes = await roomService.GetRoomTypesAsync();
        if (!ModelState.IsValid) return Page();

        var dto = new CreateRoomDto { Name = Input.Name, RoomTypeId = Input.RoomTypeId, PricePerNight = Input.PricePerNight,
            MaxOccupancy = Input.MaxOccupancy, Description = Input.Description, ImageUrl = Input.ImageUrl, Amenities = Input.Amenities };
        var result = await roomService.CreateRoomAsync(dto);

        if (result.IsSuccess)
            return RedirectToPage("/Admin/Rooms/Index");

        ErrorMessage = result.ErrorMessage;
        return Page();
    }
}
