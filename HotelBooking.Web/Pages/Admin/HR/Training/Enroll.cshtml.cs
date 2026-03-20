using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace HotelBooking.Web.Pages.Admin.HR.Training;

[Authorize(Roles = "Admin,Staff")]
public class EnrollModel(ITrainingService trainingService, IEmployeeService employeeService, IHotelService hotelService) : PageModel
{
    public IReadOnlyList<HotelDto> Hotels { get; set; } = [];
    public IReadOnlyList<EmployeeListItemDto> Employees { get; set; } = [];

    public TrainingProgramDto? Program { get; set; }

    [BindProperty] public InputModel Input { get; set; } = new();

    public string? Message { get; set; }
    public bool IsError { get; set; }

    public class InputModel
    {
        [Required] public int TrainingProgramId { get; set; }
        [Required] public int HotelId { get; set; }
        [Required] public int EmployeeId { get; set; }
    }

    public async Task OnGetAsync(int programId, int? hotelId)
    {
        Hotels = await GetScopedHotelsAsync();
        if (Hotels.Count == 0) return;

        Input.TrainingProgramId = programId;
        Input.HotelId = hotelId ?? Hotels[0].Id;

        var programRes = await trainingService.GetTrainingProgramByIdAsync(programId);
        if (programRes.IsSuccess) Program = programRes.Data;

        Employees = await LoadEmployeesAsync(Input.HotelId);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Hotels = await GetScopedHotelsAsync();
        if (Hotels.Count == 0) return Forbid();

        if (!ModelState.IsValid)
        {
            Employees = await LoadEmployeesAsync(Input.HotelId);
            return Page();
        }

        if (!User.IsInRole("Admin") && !Hotels.Any(h => h.Id == Input.HotelId))
            return Forbid();

        Employees = await LoadEmployeesAsync(Input.HotelId);

        if (!User.IsInRole("Admin"))
        {
            var allowedEmployeeIds = Employees.Select(e => e.Id).ToHashSet();
            if (!allowedEmployeeIds.Contains(Input.EmployeeId))
                return Forbid();
        }

        var result = await trainingService.EnrollEmployeeAsync(new EnrollTrainingDto
        {
            TrainingProgramId = Input.TrainingProgramId,
            EmployeeId = Input.EmployeeId
        });

        Message = result.IsSuccess ? "Employee enrolled." : result.ErrorMessage;
        IsError = !result.IsSuccess;
        return RedirectToPage("/Admin/HR/Training/Programs", new { hotelId = Input.HotelId });
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
        var r = await employeeService.GetEmployeesByHotelAsync(hotelId);
        return r.IsSuccess && r.Data is not null ? r.Data : [];
    }
}

