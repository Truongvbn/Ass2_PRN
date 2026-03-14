using HotelBooking.Business.Mappings;
using HotelBooking.Business.Services;
using HotelBooking.Business.Services.Interfaces;
using HotelBooking.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HotelBooking.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDataLayer(configuration);

        services.AddAutoMapper(typeof(MappingProfile));

        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IHotelService, HotelService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IAiAssistantService, MockAiAssistantService>();

        // Need an IAuthService implementation registered here. 
        // We'll create it.
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
