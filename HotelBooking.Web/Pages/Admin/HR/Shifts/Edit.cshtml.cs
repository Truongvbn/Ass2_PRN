using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace HotelBooking.Web.Pages.Admin.HR.Shifts;

[Authorize(Roles = "Admin,Staff")]
public class EditModel(IShiftService shiftService, IHotelService hotelService) : PageModel
{
    public WorkShiftDto? Shift { get; set; }
    public IReadOnlyList<HotelDto> Hotels { get; set; } = [];

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        public int Id { get; set; }
        public int HotelId { get; set; }

        [Required]
        public string Name { get; set; } = "";

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public bool IsOvernight { get; set; }
        public bool IsActive { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var current = await shiftService.GetShiftByIdAsync(id);
        if (!current.IsSuccess || current.Data is null)
            return NotFound();

        if (!await CanAccessHotelAsync(current.Data.HotelId))
            return Forbid();

        Shift = current.Data;
        Hotels = await GetScopedHotelsAsync();

        Input = new InputModel
        {
            Id = Shift.Id,
            HotelId = Shift.HotelId,
            Name = Shift.Name,
            StartTime = Shift.StartTime,
            EndTime = Shift.EndTime,
            IsOvernight = Shift.IsOvernight,
            IsActive = Shift.IsActive
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        if (!await CanAccessHotelAsync(Input.HotelId))
            return Forbid();

        var dto = new UpdateWorkShiftDto
        {
            Id = Input.Id,
            Name = Input.Name,
            StartTime = Input.StartTime,
            EndTime = Input.EndTime,
            IsOvernight = Input.IsOvernight,
            IsActive = Input.IsActive
        };

        var result = await shiftService.UpdateShiftAsync(dto);
        if (result.IsSuccess)
            return RedirectToPage("/Admin/HR/Shifts/Index", new { hotelId = Input.HotelId });

        ErrorMessage = result.ErrorMessage;
        return Page();
    }

    private async Task<bool> CanAccessHotelAsync(int hotelId)
    {
        if (User.IsInRole("Admin")) return true;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return false;

        var hotelsResult = await hotelService.GetHotelsByStaffAsync(userId);
        return hotelsResult.IsSuccess && hotelsResult.Data is not null && hotelsResult.Data.Any(h => h.Id == hotelId);
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
}

