using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Security.Claims;

namespace HotelBooking.Web.Pages.Admin.HR.Contracts;

[Authorize(Roles = "Admin,Staff")]
public class IndexModel(ILegalComplianceService legalService, IEmployeeService employeeService, IHotelService hotelService) : PageModel
{
    public IReadOnlyList<HotelDto> Hotels { get; set; } = [];
    public IReadOnlyList<EmployeeListItemDto> Employees { get; set; } = [];

    public IReadOnlyList<EmploymentContractDto> Contracts { get; set; } = [];
    public IReadOnlyList<InsuranceRecordDto> InsuranceRecords { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public int? HotelId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? EmployeeId { get; set; }

    public string? Message { get; set; }
    public bool IsError { get; set; }

    [BindProperty] public CreateContractInput ContractInput { get; set; } = new();
    [BindProperty] public CreateInsuranceInput InsuranceInput { get; set; } = new();

    public class CreateContractInput
    {
        public int EmployeeId { get; set; }
        public int HotelId { get; set; }
        public string ContractNumber { get; set; } = "";
        public string ContractType { get; set; } = "FixedTerm";
        public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;
        public DateTime? EndDate { get; set; }
        public decimal BaseSalary { get; set; }
        public bool InsuranceIncluded { get; set; }
        public string Status { get; set; } = "Active";
        public string? FileUrl { get; set; }
    }

    public class CreateInsuranceInput
    {
        public int EmployeeId { get; set; }
        public int HotelId { get; set; }
        public string ProviderName { get; set; } = "";
        public string PolicyNumber { get; set; } = "";
        public DateTime EffectiveDate { get; set; } = DateTime.UtcNow.Date;
        public DateTime? ExpiryDate { get; set; }
        public string? Notes { get; set; }
    }

    public async Task OnGetAsync()
    {
        Hotels = await GetScopedHotelsAsync();
        var targetHotelId = HotelId ?? Hotels.FirstOrDefault()?.Id;
        if (!targetHotelId.HasValue) return;
        HotelId = targetHotelId.Value;

        var employeesRes = await employeeService.GetEmployeesByHotelAsync(targetHotelId.Value);
        if (employeesRes.IsSuccess && employeesRes.Data is not null) Employees = employeesRes.Data;

        EmployeeId ??= Employees.FirstOrDefault()?.Id;
        if (!EmployeeId.HasValue) return;

        ContractInput.EmployeeId = EmployeeId.Value;
        ContractInput.HotelId = targetHotelId.Value;
        InsuranceInput.EmployeeId = EmployeeId.Value;
        InsuranceInput.HotelId = targetHotelId.Value;

        var contractsRes = await legalService.GetContractsByEmployeeAsync(EmployeeId.Value);
        if (contractsRes.IsSuccess && contractsRes.Data is not null) Contracts = contractsRes.Data;

        var insuranceRes = await legalService.GetInsuranceByEmployeeAsync(EmployeeId.Value);
        if (insuranceRes.IsSuccess && insuranceRes.Data is not null) InsuranceRecords = insuranceRes.Data;
    }

    public async Task<IActionResult> OnPostCreateContractAsync()
    {
        Hotels = await GetScopedHotelsAsync();
        if (Hotels.Count == 0) return Forbid();

        if (!User.IsInRole("Admin") && !Hotels.Any(h => h.Id == ContractInput.HotelId))
            return Forbid();

        if (!User.IsInRole("Admin"))
        {
            var employees = await employeeService.GetEmployeesByHotelAsync(ContractInput.HotelId);
            var allowedEmployeeIds = employees.IsSuccess && employees.Data is not null
                ? employees.Data.Select(e => e.Id).ToHashSet()
                : new HashSet<int>();
            if (!allowedEmployeeIds.Contains(ContractInput.EmployeeId))
                return Forbid();
        }

        if (!ModelState.IsValid) return Page();

        var result = await legalService.CreateContractAsync(new CreateEmploymentContractDto
        {
            EmployeeId = ContractInput.EmployeeId,
            HotelId = ContractInput.HotelId,
            ContractNumber = ContractInput.ContractNumber,
            ContractType = ContractInput.ContractType,
            StartDate = ContractInput.StartDate.Date,
            EndDate = ContractInput.EndDate?.Date,
            BaseSalary = ContractInput.BaseSalary,
            InsuranceIncluded = ContractInput.InsuranceIncluded,
            Status = ContractInput.Status,
            FileUrl = ContractInput.FileUrl
        });

        Message = result.IsSuccess ? "Contract created." : result.ErrorMessage;
        IsError = !result.IsSuccess;
        return RedirectToPage("/Admin/HR/Contracts/Index", new { hotelId = ContractInput.HotelId, employeeId = ContractInput.EmployeeId });
    }

    public async Task<IActionResult> OnPostCreateInsuranceAsync()
    {
        Hotels = await GetScopedHotelsAsync();
        if (Hotels.Count == 0) return Forbid();

        if (!ModelState.IsValid) return Page();

        if (!User.IsInRole("Admin") && !Hotels.Any(h => h.Id == InsuranceInput.HotelId))
            return Forbid();

        if (!User.IsInRole("Admin"))
        {
            var employees = await employeeService.GetEmployeesByHotelAsync(InsuranceInput.HotelId);
            var allowedEmployeeIds = employees.IsSuccess && employees.Data is not null
                ? employees.Data.Select(e => e.Id).ToHashSet()
                : new HashSet<int>();
            if (!allowedEmployeeIds.Contains(InsuranceInput.EmployeeId))
                return Forbid();
        }

        var result = await legalService.CreateInsuranceRecordAsync(new CreateInsuranceRecordDto
        {
            EmployeeId = InsuranceInput.EmployeeId,
            ProviderName = InsuranceInput.ProviderName,
            PolicyNumber = InsuranceInput.PolicyNumber,
            EffectiveDate = InsuranceInput.EffectiveDate.Date,
            ExpiryDate = InsuranceInput.ExpiryDate?.Date,
            Notes = InsuranceInput.Notes
        });

        var targetHotelId = InsuranceInput.HotelId;

        Message = result.IsSuccess ? "Insurance record created." : result.ErrorMessage;
        IsError = !result.IsSuccess;

        return RedirectToPage("/Admin/HR/Contracts/Index", new { hotelId = targetHotelId, employeeId = InsuranceInput.EmployeeId });
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

