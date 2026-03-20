using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace HotelBooking.Web.Pages.Admin.HR.Employees;

[Authorize(Roles = "Admin,Staff")]
public class EditModel(IEmployeeService employeeService, IHotelService hotelService) : PageModel
{
    public EmployeeDto? Employee { get; set; }
    public IReadOnlyList<HotelDto> Hotels { get; set; } = [];

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        public int Id { get; set; }

        public int? HotelId { get; set; }

        [Required]
        public string FullName { get; set; } = "";

        public DateTime DateOfBirth { get; set; } = DateTime.UtcNow.Date;
        public string Gender { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Email { get; set; } = "";
        public string Address { get; set; } = "";
        public string IdentityNumber { get; set; } = "";
        public string Position { get; set; } = "";
        public DateTime HireDate { get; set; } = DateTime.UtcNow.Date;
        public string EmploymentType { get; set; } = "FullTime";
        public decimal BaseSalary { get; set; }
        public string Status { get; set; } = "Active";
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        if (!await CanAccessEmployeeAsync(id))
            return Forbid();

        var result = await employeeService.GetEmployeeByIdAsync(id);
        if (!result.IsSuccess || result.Data is null)
            return NotFound();

        Employee = result.Data;
        Hotels = await GetScopedHotelsAsync();

        Input = new InputModel
        {
            Id = Employee.Id,
            HotelId = Employee.HotelId,
            FullName = Employee.FullName,
            DateOfBirth = Employee.DateOfBirth,
            Gender = Employee.Gender,
            PhoneNumber = Employee.PhoneNumber,
            Email = Employee.Email,
            Address = Employee.Address,
            IdentityNumber = Employee.IdentityNumber,
            Position = Employee.Position,
            HireDate = Employee.HireDate,
            EmploymentType = Employee.EmploymentType,
            BaseSalary = Employee.BaseSalary,
            Status = Employee.Status
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Hotels = await GetScopedHotelsAsync();

        if (!ModelState.IsValid) return Page();

        if (!await CanAccessEmployeeAsync(Input.Id))
            return Forbid();

        if (!User.IsInRole("Admin"))
        {
            if (!Input.HotelId.HasValue) return Forbid();
            if (!Hotels.Any(h => h.Id == Input.HotelId.Value)) return Forbid();
        }

        var dto = new UpdateEmployeeDto
        {
            Id = Input.Id,
            HotelId = Input.HotelId,
            FullName = Input.FullName,
            DateOfBirth = Input.DateOfBirth,
            Gender = Input.Gender,
            PhoneNumber = Input.PhoneNumber,
            Email = Input.Email,
            Address = Input.Address,
            IdentityNumber = Input.IdentityNumber,
            Position = Input.Position,
            HireDate = Input.HireDate,
            EmploymentType = Input.EmploymentType,
            BaseSalary = Input.BaseSalary,
            Status = Input.Status
        };

        var result = await employeeService.UpdateEmployeeAsync(dto);
        if (result.IsSuccess)
            return RedirectToPage("/Admin/HR/Employees/Detail", new { id = result.Data?.Id ?? Input.Id });

        ErrorMessage = result.ErrorMessage;
        Employee = result.Data;
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

    private async Task<bool> CanAccessEmployeeAsync(int employeeId)
    {
        if (User.IsInRole("Admin")) return true;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return false;

        var hotelsResult = await hotelService.GetHotelsByStaffAsync(userId);
        if (!hotelsResult.IsSuccess || hotelsResult.Data is null) return false;

        var allowedHotelIds = hotelsResult.Data.Select(h => h.Id).ToHashSet();

        var employeeResult = await employeeService.GetEmployeeByIdAsync(employeeId);
        if (!employeeResult.IsSuccess || employeeResult.Data is null) return false;

        return employeeResult.Data.HotelId.HasValue && allowedHotelIds.Contains(employeeResult.Data.HotelId.Value);
    }
}

