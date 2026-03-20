using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace HotelBooking.Web.Pages.Admin.HR.Employees;

[Authorize(Roles = "Admin,Staff")]
public class DetailModel(IEmployeeService employeeService, IHotelService hotelService) : PageModel
{
    public EmployeeDto? Employee { get; set; }

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        if (!await CanAccessEmployeeAsync(id))
            return Forbid();

        var result = await employeeService.GetEmployeeByIdAsync(id);
        if (!result.IsSuccess || result.Data is null)
            return NotFound();

        Employee = result.Data;
        return Page();
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

