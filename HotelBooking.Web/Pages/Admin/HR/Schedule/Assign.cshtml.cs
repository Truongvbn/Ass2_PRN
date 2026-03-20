using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace HotelBooking.Web.Pages.Admin.HR.Schedule;

[Authorize(Roles = "Admin,Staff")]
public class AssignModel(IShiftService shiftService, IEmployeeService employeeService, IHotelService hotelService) : PageModel
{
    public IReadOnlyList<HotelDto> Hotels { get; set; } = [];
    public IReadOnlyList<EmployeeListItemDto> Employees { get; set; } = [];
    public IReadOnlyList<WorkShiftDto> Shifts { get; set; } = [];

    [BindProperty] public InputModel Input { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required] public int HotelId { get; set; }
        [Required] public int EmployeeId { get; set; }
        [Required] public int WorkShiftId { get; set; }
        [Required] public DateTime ShiftDate { get; set; } = DateTime.UtcNow.Date;
        public string? Notes { get; set; }
    }

    public async Task OnGetAsync(int? hotelId)
    {
        Hotels = await GetScopedHotelsAsync();
        if (Hotels.Count == 0) return;

        Input.HotelId = hotelId ?? Hotels[0].Id;
        await LoadEmployeesAndShiftsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Hotels = await GetScopedHotelsAsync();
        if (Hotels.Count == 0) return Forbid();
        if (!ModelState.IsValid)
        {
            await LoadEmployeesAndShiftsAsync();
            return Page();
        }

        if (!User.IsInRole("Admin") && !await IsHotelAllowedAsync(Input.HotelId))
            return Forbid();

        await LoadEmployeesAndShiftsAsync();

        if (!User.IsInRole("Admin"))
        {
            var allowedEmployeeIds = Employees.Select(e => e.Id).ToHashSet();
            var allowedShiftIds = Shifts.Select(s => s.Id).ToHashSet();

            if (!allowedEmployeeIds.Contains(Input.EmployeeId))
                return Forbid();
            if (!allowedShiftIds.Contains(Input.WorkShiftId))
                return Forbid();
        }

        var result = await shiftService.AssignShiftAsync(new CreateShiftAssignmentDto
        {
            HotelId = Input.HotelId,
            EmployeeId = Input.EmployeeId,
            WorkShiftId = Input.WorkShiftId,
            ShiftDate = Input.ShiftDate.Date,
            Notes = Input.Notes
        });

        if (result.IsSuccess)
            return RedirectToPage("/Admin/HR/Schedule/ByHotel", new { hotelId = Input.HotelId, startDate = Input.ShiftDate.Date, endDate = Input.ShiftDate.Date });

        ErrorMessage = result.ErrorMessage;
        return Page();
    }

    private async Task LoadEmployeesAndShiftsAsync()
    {
        if (Input.HotelId == 0) return;
        var employeesResult = await employeeService.GetEmployeesByHotelAsync(Input.HotelId);
        Employees = employeesResult.IsSuccess && employeesResult.Data is not null ? employeesResult.Data : [];

        var shiftsResult = await shiftService.GetShiftsByHotelAsync(Input.HotelId);
        Shifts = shiftsResult.IsSuccess && shiftsResult.Data is not null ? shiftsResult.Data : [];
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
        var scoped = await hotelService.GetHotelsByStaffAsync(userId);
        return scoped.IsSuccess && scoped.Data is not null && scoped.Data.Any(h => h.Id == hotelId);
    }
}

