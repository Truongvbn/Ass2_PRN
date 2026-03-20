using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace HotelBooking.Web.Pages.Admin.HR.Attendance;

[Authorize(Roles = "Admin,Staff")]
public class IndexModel(IAttendanceService attendanceService, IEmployeeService employeeService, IHotelService hotelService) : PageModel
{
    public IReadOnlyList<HotelDto> Hotels { get; set; } = [];
    public IReadOnlyList<EmployeeListItemDto> Employees { get; set; } = [];
    public IReadOnlyList<AttendanceDto> Records { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public int? HotelId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? EmployeeId { get; set; }

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

        EmployeeId ??= null;

        Records = [];
        var start = (StartDate ?? DateTime.UtcNow.Date).Date;
        var end = (EndDate ?? DateTime.UtcNow.Date.AddDays(7)).Date;
        if (end < start) end = start;

        if (EmployeeId.HasValue)
        {
            var r = await attendanceService.GetAttendanceByEmployeeAsync(EmployeeId.Value, start, end);
            if (r.IsSuccess && r.Data is not null) Records = r.Data;
        }
        else
        {
            var r = await attendanceService.GetAttendanceByHotelAsync(targetHotelId.Value, start, end);
            if (r.IsSuccess && r.Data is not null) Records = r.Data;
        }

        if (targetHotelId.HasValue)
        {
            var empResult = await employeeService.GetEmployeesByHotelAsync(targetHotelId.Value);
            if (empResult.IsSuccess && empResult.Data is not null) Employees = empResult.Data;
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

