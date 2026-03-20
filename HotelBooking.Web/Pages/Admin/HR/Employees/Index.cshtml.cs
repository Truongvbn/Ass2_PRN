using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace HotelBooking.Web.Pages.Admin.HR.Employees;

[Authorize(Roles = "Admin,Staff")]
public class IndexModel(IEmployeeService employeeService, IHotelService hotelService) : PageModel
{
    public IReadOnlyList<EmployeeListItemDto> Employees { get; set; } = [];
    public IReadOnlyList<HotelDto> Hotels { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public int? HotelId { get; set; }

    public int? SelectedHotelId { get; set; }
    public string? Message { get; set; }
    public bool IsError { get; set; }

    public async Task OnGetAsync()
    {
        var scopedHotels = await GetScopedHotelsAsync();
        Hotels = scopedHotels;

        var targetHotelId = HotelId ?? scopedHotels.FirstOrDefault()?.Id;
        SelectedHotelId = targetHotelId;

        if (targetHotelId.HasValue)
        {
            var employeesResult = await employeeService.GetEmployeesByHotelAsync(targetHotelId.Value);
            if (employeesResult.IsSuccess && employeesResult.Data is not null)
            {
                Employees = employeesResult.Data;
            }
        }
    }

    public async Task<IActionResult> OnPostDeactivateAsync(int id)
    {
        if (!await CanAccessEmployeeAsync(id)) return Forbid();
        var result = await employeeService.DeactivateEmployeeAsync(id);
        Message = result.IsSuccess ? "Employee deactivated." : result.ErrorMessage;
        IsError = !result.IsSuccess;
        await OnGetAsync();
        return Page();
    }

    private async Task<IReadOnlyList<HotelDto>> GetScopedHotelsAsync()
    {
        if (User.IsInRole("Admin"))
        {
            var allHotels = await hotelService.GetAllHotelsAsync();
            return allHotels.IsSuccess && allHotels.Data is not null ? allHotels.Data : [];
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return [];

        var hotels = await hotelService.GetHotelsByStaffAsync(userId);
        return hotels.IsSuccess && hotels.Data is not null ? hotels.Data : [];
    }

    private async Task<bool> CanAccessEmployeeAsync(int employeeId)
    {
        if (User.IsInRole("Admin")) return true;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return false;

        var hotels = await hotelService.GetHotelsByStaffAsync(userId);
        if (!hotels.IsSuccess || hotels.Data is null) return false;
        var allowedHotelIds = hotels.Data.Select(h => h.Id).ToHashSet();

        var employeeResult = await employeeService.GetEmployeeByIdAsync(employeeId);
        if (!employeeResult.IsSuccess || employeeResult.Data is null) return false;

        return employeeResult.Data.HotelId.HasValue && allowedHotelIds.Contains(employeeResult.Data.HotelId.Value);
    }
}

