using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text;

namespace HotelBooking.Web.Pages.Reports;

[Authorize(Roles = "Admin,Staff")]
public class FinancialModel(
    IReportingService reportingService,
    IHotelService hotelService) : PageModel
{
    public FinancialReportDto? Report { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public DateTime StartDate { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
    
    [BindProperty(SupportsGet = true)]
    public DateTime EndDate { get; set; } = DateTime.Now;

    [BindProperty(SupportsGet = true)]
    public int? HotelId { get; set; }

    public List<HotelDto> AvailableHotels { get; set; } = new();

    public async Task OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isAdmin = User.IsInRole("Admin");

        if (isAdmin)
        {
            var hotelsResult = await hotelService.GetAllHotelsAsync();
            AvailableHotels = hotelsResult.Data?.ToList() ?? new();
            
            if (HotelId.HasValue)
            {
                var result = await reportingService.GetHotelFinancialReportAsync(HotelId.Value, StartDate, EndDate);
                Report = result.Data;
            }
            else
            {
                var result = await reportingService.GetAdminFinancialReportAsync(StartDate, EndDate);
                Report = result.Data;
            }
        }
        else
        {
            var hotelsResult = await hotelService.GetHotelsByStaffAsync(userId);
            AvailableHotels = hotelsResult.Data?.ToList() ?? new();
            
            if (AvailableHotels.Count > 0)
            {
                HotelId = AvailableHotels[0].Id;
                var result = await reportingService.GetHotelFinancialReportAsync(HotelId.Value, StartDate, EndDate);
                Report = result.Data;
            }
        }
    }

    public async Task<IActionResult> OnGetExportCsvAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isAdmin = User.IsInRole("Admin");
        FinancialReportDto? reportData = null;

        if (isAdmin)
        {
            if (HotelId.HasValue)
            {
                var result = await reportingService.GetHotelFinancialReportAsync(HotelId.Value, StartDate, EndDate);
                reportData = result.Data;
            }
            else
            {
                var result = await reportingService.GetAdminFinancialReportAsync(StartDate, EndDate);
                reportData = result.Data;
            }
        }
        else
        {
            var hotelsResult = await hotelService.GetHotelsByStaffAsync(userId);
            if (hotelsResult.Data?.Count > 0)
            {
                var result = await reportingService.GetHotelFinancialReportAsync(hotelsResult.Data[0].Id, StartDate, EndDate);
                reportData = result.Data;
            }
        }

        if (reportData == null) return NotFound();

        var csv = new StringBuilder();
        csv.AppendLine("Date,Revenue,Bookings");
        foreach (var day in reportData.DailyRevenues)
        {
            csv.AppendLine($"{day.Date:yyyy-MM-dd},{day.Revenue},{day.BookingCount}");
        }

        var fileName = $"FinancialReport_{StartDate:yyyyMMdd}_{EndDate:yyyyMMdd}.csv";
        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
    }
}
