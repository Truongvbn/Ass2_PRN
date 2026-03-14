using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace HotelBooking.Web.Pages.Admin.Rooms;

[Authorize(Roles = "Admin,Staff")]
public class EditModel(IRoomService roomService, IHotelService hotelService) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();
    public IReadOnlyList<RoomTypeDto> RoomTypes { get; set; } = [];
    public IReadOnlyList<HotelDto> Hotels { get; set; } = [];
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        public int Id { get; set; }
        [Required] public int HotelId { get; set; }
        [Required] public string Name { get; set; } = "";
        [Required] public int RoomTypeId { get; set; }
        [Required, Range(1, 10000)] public decimal PricePerNight { get; set; }
        [Required, Range(1, 50)] public int MaxOccupancy { get; set; }
        public string ImageUrl { get; set; } = "";
        public string Amenities { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsAvailable { get; set; }
    }

    public async Task OnGetAsync(int id)
    {
        RoomTypes = await roomService.GetRoomTypesAsync();
        await LoadHotelsAsync();
        var result = await roomService.GetRoomByIdAsync(id);
        if (result.IsSuccess && result.Data != null)
        {
            var r = result.Data;
            Input = new InputModel
            {
                Id = r.Id, HotelId = r.HotelId, Name = r.Name, RoomTypeId = r.RoomTypeId,
                PricePerNight = r.PricePerNight, MaxOccupancy = r.MaxOccupancy,
                ImageUrl = r.ImageUrl, Amenities = r.Amenities, Description = r.Description,
                IsAvailable = r.IsAvailable
            };
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        RoomTypes = await roomService.GetRoomTypesAsync();
        await LoadHotelsAsync();
        if (!ModelState.IsValid) return Page();

        if (!await CanAccessHotelAsync(Input.HotelId)) return Forbid();

        var dto = new UpdateRoomDto 
        { 
            Id = Input.Id, 
            HotelId = Input.HotelId,
            Name = Input.Name, 
            RoomTypeId = Input.RoomTypeId, 
            PricePerNight = Input.PricePerNight, 
            MaxOccupancy = Input.MaxOccupancy, 
            Description = Input.Description, 
            ImageUrl = Input.ImageUrl, 
            Amenities = Input.Amenities, 
            IsAvailable = Input.IsAvailable 
        };
        var result = await roomService.UpdateRoomAsync(dto);

        if (result.IsSuccess)
            return RedirectToPage("/Admin/Rooms/Index");

        ErrorMessage = result.ErrorMessage;
        return Page();
    }

    private async Task LoadHotelsAsync()
    {
        if (User.IsInRole("Admin"))
        {
            var all = await hotelService.GetAllHotelsAsync();
            Hotels = all.IsSuccess && all.Data is not null ? all.Data : [];
            return;
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return;

        var scoped = await hotelService.GetHotelsByStaffAsync(userId);
        Hotels = scoped.IsSuccess && scoped.Data is not null ? scoped.Data : [];
    }

    private async Task<bool> CanAccessHotelAsync(int hotelId)
    {
        if (User.IsInRole("Admin")) return true;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return false;

        var hotelsResult = await hotelService.GetHotelsByStaffAsync(userId);
        if (!hotelsResult.IsSuccess || hotelsResult.Data is null) return false;

        return hotelsResult.Data.Any(h => h.Id == hotelId);
    }
}
