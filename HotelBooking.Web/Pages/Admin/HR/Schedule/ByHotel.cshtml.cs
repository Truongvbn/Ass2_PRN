using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace HotelBooking.Web.Pages.Admin.HR.Schedule;

[Authorize(Roles = "Admin,Staff")]
public class ByHotelModel(IShiftService shiftService, IHotelService hotelService) : PageModel
{
    public IReadOnlyList<HotelDto> Hotels { get; set; } = [];
    public IReadOnlyList<ShiftAssignmentDto> Assignments { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public int? HotelId { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? StartDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? EndDate { get; set; }

    public string? Message { get; set; }
    public bool IsError { get; set; }

    public async Task OnGetAsync()
    {
        Hotels = await GetScopedHotelsAsync();
        var targetHotelId = HotelId ?? Hotels.FirstOrDefault()?.Id;
        if (!targetHotelId.HasValue) return;

        var start = (StartDate ?? DateTime.UtcNow.Date).Date;
        var end = (EndDate ?? DateTime.UtcNow.Date.AddDays(7)).Date;

        if (end < start) end = start;

        var result = await shiftService.GetScheduleByHotelAsync(targetHotelId.Value, start, end);
        if (result.IsSuccess && result.Data is not null)
            Assignments = result.Data;
        else
        {
            Message = result.ErrorMessage;
            IsError = true;
        }
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

