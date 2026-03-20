using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace HotelBooking.Web.Pages.Admin.HR.Payroll;

[Authorize(Roles = "Admin,Staff")]
public class DetailsModel(IPayrollService payrollService, IHotelService hotelService) : PageModel
{
    public int Id { get; set; }
    public int HotelId { get; set; }
    public PayrollPeriodDto? Period { get; set; }
    public IReadOnlyList<PayrollEntryDto> Entries { get; set; } = [];

    public string? Message { get; set; }
    public bool IsError { get; set; }

    public async Task<IActionResult> OnGetAsync(int id, int hotelId)
    {
        if (!await CanAccessHotelAsync(hotelId))
            return Forbid();

        Id = id;
        HotelId = hotelId;

        var periods = await payrollService.GetPayrollPeriodsByHotelAsync(hotelId);
        if (!periods.IsSuccess || periods.Data is null)
            return NotFound();

        Period = periods.Data.FirstOrDefault(p => p.Id == id);
        if (Period is null) return NotFound();

        var entries = await payrollService.GetPayrollEntriesAsync(id);
        if (entries.IsSuccess && entries.Data is not null)
            Entries = entries.Data;

        return Page();
    }

    public async Task<IActionResult> OnPostCalculateAsync(int id, int hotelId)
    {
        if (!await CanAccessPayrollPeriodAsync(id, hotelId))
            return Forbid();

        var result = await payrollService.CalculatePayrollAsync(id);
        Message = result.IsSuccess ? "Payroll calculated." : result.ErrorMessage;
        IsError = !result.IsSuccess;

        return RedirectToPage("/Admin/HR/Payroll/Details", new { id, hotelId });
    }

    public async Task<IActionResult> OnPostApproveAsync(int id, int hotelId)
    {
        if (!await CanAccessPayrollPeriodAsync(id, hotelId))
            return Forbid();

        var result = await payrollService.ApprovePayrollAsync(id);
        Message = result.IsSuccess ? "Payroll approved." : result.ErrorMessage;
        IsError = !result.IsSuccess;
        return RedirectToPage("/Admin/HR/Payroll/Details", new { id, hotelId });
    }

    public async Task<IActionResult> OnPostMarkPaidAsync(int id, int hotelId)
    {
        if (!await CanAccessPayrollPeriodAsync(id, hotelId))
            return Forbid();

        var result = await payrollService.MarkPayrollPaidAsync(id);
        Message = result.IsSuccess ? "Payroll marked paid." : result.ErrorMessage;
        IsError = !result.IsSuccess;
        return RedirectToPage("/Admin/HR/Payroll/Details", new { id, hotelId });
    }

    private async Task<bool> CanAccessHotelAsync(int hotelId)
    {
        if (User.IsInRole("Admin")) return true;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return false;
        var hotelsResult = await hotelService.GetHotelsByStaffAsync(userId);
        return hotelsResult.IsSuccess && hotelsResult.Data is not null && hotelsResult.Data.Any(h => h.Id == hotelId);
    }

    private async Task<bool> CanAccessPayrollPeriodAsync(int payrollPeriodId, int hotelId)
    {
        if (!await CanAccessHotelAsync(hotelId)) return false;

        var periods = await payrollService.GetPayrollPeriodsByHotelAsync(hotelId);
        return periods.IsSuccess && periods.Data is not null && periods.Data.Any(p => p.Id == payrollPeriodId);
    }
}

