using HotelBooking.Data.Entities;
using HotelBooking.Data.Repositories.Interfaces;
using HotelBooking.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace HotelBooking.Web.Services;

public class BookingExpirationService : BackgroundService
{
    private readonly IServiceProvider _services;
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

    public BookingExpirationService(IServiceProvider services)
    {
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExpireBookingsAsync(stoppingToken);
            }
            catch
            {
                // Log and continue
            }
            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ExpireBookingsAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var bookingRepo = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<BookingHub>>();

        var utcNow = DateTime.UtcNow;

        var pendingExpired = await bookingRepo.GetExpiredPendingAsync(utcNow, ct);
        foreach (var b in pendingExpired)
        {
            b.Status = BookingStatus.Expired;
            b.UpdatedAt = utcNow;
            await bookingRepo.UpdateAsync(b, ct);
            await hubContext.Clients.User(b.UserId).SendAsync("BookingExpired", b.Id, ct);
        }

        var awaitingExpired = await bookingRepo.GetExpiredAwaitingPaymentAsync(utcNow, ct);
        foreach (var b in awaitingExpired)
        {
            b.Status = BookingStatus.Expired;
            b.UpdatedAt = utcNow;
            await bookingRepo.UpdateAsync(b, ct);
            await hubContext.Clients.User(b.UserId).SendAsync("BookingExpired", b.Id, ct);
        }
    }
}
