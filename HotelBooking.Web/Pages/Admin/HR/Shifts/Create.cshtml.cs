using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace HotelBooking.Web.Pages.Admin.HR.Shifts;

[Authorize(Roles = "Admin,Staff")]
public class CreateModel(IShiftService shiftService, IHotelService hotelService) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();

    public IReadOnlyList<HotelDto> Hotels { get; set; } = [];
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required] public int HotelId { get; set; }
        [Required] public string Name { get; set; } = "";
        [Required] public TimeSpan StartTime { get; set; }
        [Required] public TimeSpan EndTime { get; set; }
        public bool IsOvernight { get; set; }
    }

    public async Task OnGetAsync()
    {
        Hotels = await GetScopedHotelsAsync();
        if (Hotels.Count > 0 && Input.HotelId == 0)
            Input.HotelId = Hotels[0].Id;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Hotels = await GetScopedHotelsAsync();
        if (!ModelState.IsValid) return Page();

        // Staff cannot create shifts outside their assigned hotels
        if (!User.IsInRole("Admin") && !await IsHotelAllowedAsync(Input.HotelId))
            return Forbid();

        var dto = new CreateWorkShiftDto
        {
            HotelId = Input.HotelId,
            Name = Input.Name,
            StartTime = Input.StartTime,
            EndTime = Input.EndTime,
            IsOvernight = Input.IsOvernight
        };

        var result = await shiftService.CreateShiftAsync(dto);
        if (result.IsSuccess)
            return RedirectToPage("/Admin/HR/Shifts/Index", new { hotelId = Input.HotelId });

        ErrorMessage = result.ErrorMessage;
        return Page();
    }

    private async Task<IReadOnlyList<HotelDto>> GetScopedHotelsAsync()
    {
        if (User.IsInRole("Admin"))
        {
            var all = await hotelService.GetAllHotelsAsync();
            return all.IsSuccess && all.Data is not null ? all.Data : [];
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return [];
        var scoped = await hotelService.GetHotelsByStaffAsync(userId);
        return scoped.IsSuccess && scoped.Data is not null ? scoped.Data : [];
    }

    private async Task<bool> IsHotelAllowedAsync(int hotelId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return false;

        var hotelsResult = await hotelService.GetHotelsByStaffAsync(userId);
        return hotelsResult.IsSuccess && hotelsResult.Data is not null && hotelsResult.Data.Any(h => h.Id == hotelId);
    }
}

