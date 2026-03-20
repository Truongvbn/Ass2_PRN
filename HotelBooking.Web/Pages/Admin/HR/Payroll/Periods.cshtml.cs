using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace HotelBooking.Web.Pages.Admin.HR.Payroll;

[Authorize(Roles = "Admin,Staff")]
public class PeriodsModel(IPayrollService payrollService, IHotelService hotelService) : PageModel
{
    public IReadOnlyList<HotelDto> Hotels { get; set; } = [];
    public IReadOnlyList<PayrollPeriodDto> Periods { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public int? HotelId { get; set; }

    public int SelectedHotelId => HotelId ?? 0;

    public string? Message { get; set; }
    public bool IsError { get; set; }

    [BindProperty] public CreatePeriodInput Input { get; set; } = new();

    public class CreatePeriodInput
    {
        public int HotelId { get; set; }
        public string Name { get; set; } = "";
        public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;
        public DateTime EndDate { get; set; } = DateTime.UtcNow.Date.AddDays(30);
    }

    public async Task OnGetAsync()
    {
        Hotels = await GetScopedHotelsAsync();
        var targetHotelId = HotelId ?? Hotels.FirstOrDefault()?.Id;
        if (!targetHotelId.HasValue) return;

        HotelId = targetHotelId;
        Input.HotelId = targetHotelId.Value;

        var result = await payrollService.GetPayrollPeriodsByHotelAsync(targetHotelId.Value);
        if (result.IsSuccess && result.Data is not null) Periods = result.Data;
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        Hotels = await GetScopedHotelsAsync();
        if (Hotels.Count == 0) return Forbid();

        if (!ModelState.IsValid)
            return Page();

        if (!User.IsInRole("Admin") && !Hotels.Any(h => h.Id == Input.HotelId))
            return Forbid();

        if (Input.EndDate.Date <= Input.StartDate.Date)
        {
            Message = "EndDate must be after StartDate.";
            IsError = true;
            return Page();
        }

        var result = await payrollService.CreatePayrollPeriodAsync(new CreatePayrollPeriodDto
        {
            HotelId = Input.HotelId,
            Name = Input.Name,
            StartDate = Input.StartDate.Date,
            EndDate = Input.EndDate.Date
        });

        Message = result.IsSuccess ? "Payroll period created." : result.ErrorMessage;
        IsError = !result.IsSuccess;

        return RedirectToPage("/Admin/HR/Payroll/Periods", new { hotelId = Input.HotelId });
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

