using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Security.Claims;

namespace HotelBooking.Web.Pages.Admin.HR.Performance;

[Authorize(Roles = "Admin,Staff")]
public class ReviewsModel(IPerformanceService performanceService, IEmployeeService employeeService, IHotelService hotelService) : PageModel
{
    public IReadOnlyList<HotelDto> Hotels { get; set; } = [];
    public IReadOnlyList<EmployeeListItemDto> Employees { get; set; } = [];
    public IReadOnlyList<PerformanceReviewDto> Reviews { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public int? HotelId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? EmployeeId { get; set; }

    public string? Message { get; set; }
    public bool IsError { get; set; }

    [BindProperty] public CreateReviewInput Input { get; set; } = new();

    public class CreateReviewInput
    {
        public int HotelId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime ReviewDate { get; set; } = DateTime.UtcNow.Date;
        public DateTime PeriodStart { get; set; } = DateTime.UtcNow.Date.AddDays(-30);
        public DateTime PeriodEnd { get; set; } = DateTime.UtcNow.Date;
        [Range(1, 5)] public int OverallRating { get; set; } = 3;
        public string Strengths { get; set; } = "";
        public string Improvements { get; set; } = "";
        public string Goals { get; set; } = "";
    }

    public async Task OnGetAsync()
    {
        Hotels = await GetScopedHotelsAsync();
        var targetHotelId = HotelId ?? Hotels.FirstOrDefault()?.Id;
        if (!targetHotelId.HasValue) return;
        HotelId = targetHotelId.Value;

        var employeesRes = await employeeService.GetEmployeesByHotelAsync(targetHotelId.Value);
        if (employeesRes.IsSuccess && employeesRes.Data is not null)
            Employees = employeesRes.Data;

        EmployeeId ??= Employees.FirstOrDefault()?.Id;
        if (!EmployeeId.HasValue) return;

        Input.HotelId = targetHotelId.Value;
        Input.EmployeeId = EmployeeId.Value;

        var reviewsRes = await performanceService.GetReviewsByEmployeeAsync(EmployeeId.Value);
        if (reviewsRes.IsSuccess && reviewsRes.Data is not null)
            Reviews = reviewsRes.Data;
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        Hotels = await GetScopedHotelsAsync();
        var targetHotelId = HotelId ?? Hotels.FirstOrDefault()?.Id;
        if (!targetHotelId.HasValue) return Forbid();

        Input.HotelId = Input.HotelId == 0 ? targetHotelId.Value : Input.HotelId;
        if (!User.IsInRole("Admin") && !Hotels.Any(h => h.Id == Input.HotelId))
            return Forbid();

        if (!ModelState.IsValid)
        {
            var employeesRes = await employeeService.GetEmployeesByHotelAsync(Input.HotelId);
            Employees = employeesRes.IsSuccess && employeesRes.Data is not null ? employeesRes.Data : [];
            return Page();
        }

        if (!User.IsInRole("Admin"))
        {
            var employeesRes = await employeeService.GetEmployeesByHotelAsync(Input.HotelId);
            var allowedEmployeeIds = employeesRes.IsSuccess && employeesRes.Data is not null
                ? employeesRes.Data.Select(e => e.Id).ToHashSet()
                : new HashSet<int>();

            if (!allowedEmployeeIds.Contains(Input.EmployeeId))
                return Forbid();
        }

        var reviewerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(reviewerId)) return Forbid();

        var dto = new CreatePerformanceReviewDto
        {
            EmployeeId = Input.EmployeeId,
            HotelId = Input.HotelId,
            ReviewDate = Input.ReviewDate.Date,
            PeriodStart = Input.PeriodStart.Date,
            PeriodEnd = Input.PeriodEnd.Date,
            OverallRating = Input.OverallRating,
            Strengths = Input.Strengths,
            Improvements = Input.Improvements,
            Goals = Input.Goals
        };

        var result = await performanceService.CreateReviewAsync(dto, reviewerId);
        Message = result.IsSuccess ? "Performance review created." : result.ErrorMessage;
        IsError = !result.IsSuccess;

        return RedirectToPage("/Admin/HR/Performance/Reviews", new { hotelId = Input.HotelId, employeeId = Input.EmployeeId });
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

