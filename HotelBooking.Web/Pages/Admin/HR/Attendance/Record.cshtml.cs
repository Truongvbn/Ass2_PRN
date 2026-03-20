using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace HotelBooking.Web.Pages.Admin.HR.Attendance;

[Authorize(Roles = "Admin,Staff")]
public class RecordModel(IAttendanceService attendanceService, IEmployeeService employeeService, IShiftService shiftService, IHotelService hotelService)
    : PageModel
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
        [Required] public DateTime ShiftDate { get; set; } = DateTime.UtcNow.Date;
        public int? WorkShiftId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? CheckInTime { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? CheckOutTime { get; set; }

        public string? Notes { get; set; }
    }

    public async Task OnGetAsync(int hotelId, int employeeId, DateTime shiftDate)
    {
        Hotels = await GetScopedHotelsAsync();
        if (Hotels.Count == 0) return;

        Input.HotelId = hotelId;
        Input.EmployeeId = employeeId;
        Input.ShiftDate = shiftDate.Date;

        Employees = await LoadEmployeesAsync(Input.HotelId);
        Shifts = await LoadShiftsAsync(Input.HotelId);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Hotels = await GetScopedHotelsAsync();
        if (Hotels.Count == 0) return Forbid();

        if (!ModelState.IsValid)
        {
            Employees = await LoadEmployeesAsync(Input.HotelId);
            Shifts = await LoadShiftsAsync(Input.HotelId);
            return Page();
        }

        if (!User.IsInRole("Admin") && !await IsHotelAllowedAsync(Input.HotelId))
            return Forbid();

        Employees = await LoadEmployeesAsync(Input.HotelId);
        Shifts = await LoadShiftsAsync(Input.HotelId);

        if (!User.IsInRole("Admin"))
        {
            var allowedEmployeeIds = Employees.Select(e => e.Id).ToHashSet();
            if (!allowedEmployeeIds.Contains(Input.EmployeeId))
                return Forbid();

            if (Input.WorkShiftId.HasValue)
            {
                var allowedShiftIds = Shifts.Select(s => s.Id).ToHashSet();
                if (!allowedShiftIds.Contains(Input.WorkShiftId.Value))
                    return Forbid();
            }
        }

        var dto = new RecordAttendanceDto
        {
            HotelId = Input.HotelId,
            EmployeeId = Input.EmployeeId,
            ShiftDate = Input.ShiftDate.Date,
            WorkShiftId = Input.WorkShiftId,
            CheckInTime = Input.CheckInTime,
            CheckOutTime = Input.CheckOutTime,
            Notes = Input.Notes
        };

        var result = await attendanceService.RecordAttendanceAsync(dto);
        if (!result.IsSuccess)
        {
            ErrorMessage = result.ErrorMessage;
            return Page();
        }

        return RedirectToPage("/Admin/HR/Attendance/Index", new
        {
            hotelId = Input.HotelId,
            employeeId = Input.EmployeeId,
            startDate = Input.ShiftDate.Date,
            endDate = Input.ShiftDate.Date
        });
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

    private async Task<IReadOnlyList<EmployeeListItemDto>> LoadEmployeesAsync(int hotelId)
    {
        var result = await employeeService.GetEmployeesByHotelAsync(hotelId);
        return result.IsSuccess && result.Data is not null ? result.Data : [];
    }

    private async Task<IReadOnlyList<WorkShiftDto>> LoadShiftsAsync(int hotelId)
    {
        var result = await shiftService.GetShiftsByHotelAsync(hotelId);
        return result.IsSuccess && result.Data is not null ? result.Data : [];
    }

    private async Task<bool> IsHotelAllowedAsync(int hotelId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return false;
        var scoped = await hotelService.GetHotelsByStaffAsync(userId);
        return scoped.IsSuccess && scoped.Data is not null && scoped.Data.Any(h => h.Id == hotelId);
    }
}

