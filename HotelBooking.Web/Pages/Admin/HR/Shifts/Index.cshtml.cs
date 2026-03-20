using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace HotelBooking.Web.Pages.Admin.HR.Shifts;

[Authorize(Roles = "Admin,Staff")]
public class IndexModel(IShiftService shiftService, IHotelService hotelService) : PageModel
{
    public IReadOnlyList<WorkShiftDto> Shifts { get; set; } = [];
    public IReadOnlyList<HotelDto> Hotels { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public int? HotelId { get; set; }

    public int? SelectedHotelId { get; set; }
    public string? Message { get; set; }
    public bool IsError { get; set; }

    public async Task OnGetAsync()
    {
        Hotels = await GetScopedHotelsAsync();
        SelectedHotelId = HotelId ?? Hotels.FirstOrDefault()?.Id;

        if (!SelectedHotelId.HasValue) return;

        var result = await shiftService.GetShiftsByHotelAsync(SelectedHotelId.Value);
        if (result.IsSuccess && result.Data is not null)
            Shifts = result.Data;
    }

    public async Task<IActionResult> OnPostToggleActiveAsync(int id)
    {
        if (!await CanAccessShiftAsync(id))
            return Forbid();

        var current = await shiftService.GetShiftByIdAsync(id);
        if (!current.IsSuccess || current.Data is null)
        {
            Message = current.ErrorMessage;
            IsError = true;
            await OnGetAsync();
            return Page();
        }

        var result = await shiftService.ToggleShiftActiveAsync(id, !current.Data.IsActive);
        Message = result.IsSuccess ? "Shift updated." : result.ErrorMessage;
        IsError = !result.IsSuccess;

        await OnGetAsync();
        return Page();
    }

    private async Task<bool> CanAccessShiftAsync(int shiftId)
    {
        if (User.IsInRole("Admin")) return true;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return false;

        var hotelsResult = await hotelService.GetHotelsByStaffAsync(userId);
        if (!hotelsResult.IsSuccess || hotelsResult.Data is null) return false;
        var allowedHotelIds = hotelsResult.Data.Select(h => h.Id).ToHashSet();

        var shift = await shiftService.GetShiftByIdAsync(shiftId);
        if (!shift.IsSuccess || shift.Data is null) return false;

        return allowedHotelIds.Contains(shift.Data.HotelId);
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

