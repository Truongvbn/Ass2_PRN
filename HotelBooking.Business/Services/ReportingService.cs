using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using HotelBooking.Data.Entities;
using HotelBooking.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Business.Services;

public class ReportingService(
    IBookingRepository bookingRepo) : IReportingService
{
    public async Task<ServiceResult<FinancialReportDto>> GetAdminFinancialReportAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var bookings = await bookingRepo.GetAllWithDetailsAsync(ct);
        var filteredBookings = bookings
            .Where(b => b.CreatedAt >= startDate && b.CreatedAt <= endDate)
            .ToList();

        var report = GenerateReport(filteredBookings);

        // Add hotel breakdown for admin
        var hotelGroups = filteredBookings
            .GroupBy(b => new { b.Room.HotelId, b.Room.Hotel.Name })
            .Select(g => new HotelFinancialReportDto
            {
                HotelId = g.Key.HotelId,
                HotelName = g.Key.Name,
                TotalRevenue = g.Where(b => b.Status == BookingStatus.Completed || b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.CheckedIn)
                                .Sum(b => b.TotalPrice + (b.IsExtraChargePaid ? b.ExtraChargeAmount : 0) - (b.Payment?.RefundAmount ?? 0)),
                BookingCount = g.Count()
            })
            .OrderByDescending(h => h.TotalRevenue)
            .ToList();

        return ServiceResult<FinancialReportDto>.Success(report with { HotelReports = hotelGroups });
    }

    public async Task<ServiceResult<FinancialReportDto>> GetHotelFinancialReportAsync(int hotelId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var bookings = await bookingRepo.GetByHotelAsync(hotelId);
        var filteredBookings = bookings
            .Where(b => b.CreatedAt >= startDate && b.CreatedAt <= endDate)
            .ToList();

        var report = GenerateReport(filteredBookings);
        return ServiceResult<FinancialReportDto>.Success(report);
    }

    private FinancialReportDto GenerateReport(List<Booking> bookings)
    {
        var completedStatuses = new[] { BookingStatus.Completed, BookingStatus.Confirmed, BookingStatus.CheckedIn };
        
        var successfulBookings = bookings.Where(b => completedStatuses.Contains(b.Status)).ToList();
        
        var bookingRevenue = successfulBookings.Sum(b => b.TotalPrice);
        var extraChargeRevenue = successfulBookings.Where(b => b.IsExtraChargePaid).Sum(b => b.ExtraChargeAmount);
        var totalRefunds = bookings.Sum(b => b.Payment?.RefundAmount ?? 0);
        
        var totalRevenue = bookingRevenue + extraChargeRevenue - totalRefunds;

        var dailyRevenues = bookings
            .GroupBy(b => b.CreatedAt.Date)
            .Select(g => new DailyRevenueDto
            {
                Date = g.Key,
                Revenue = g.Where(b => completedStatuses.Contains(b.Status))
                           .Sum(b => b.TotalPrice + (b.IsExtraChargePaid ? b.ExtraChargeAmount : 0) - (b.Payment?.RefundAmount ?? 0)),
                BookingCount = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToList();

        return new FinancialReportDto
        {
            TotalRevenue = totalRevenue,
            BookingRevenue = bookingRevenue,
            ExtraChargeRevenue = extraChargeRevenue,
            RefundAmount = totalRefunds,
            TotalBookings = bookings.Count,
            CompletedBookings = bookings.Count(b => b.Status == BookingStatus.Completed),
            CancelledBookings = bookings.Count(b => b.Status == BookingStatus.Cancelled),
            DailyRevenues = dailyRevenues
        };
    }
}
