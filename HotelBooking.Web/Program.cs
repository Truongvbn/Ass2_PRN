using HotelBooking.Business;
using HotelBooking.Business.Mappings;
using HotelBooking.Business.Services;
using HotelBooking.Business.Services.Interfaces;
using HotelBooking.Data;
using HotelBooking.Data.Entities;
using HotelBooking.Data.Repositories;
using HotelBooking.Data.Repositories.Interfaces;
using HotelBooking.Web.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Business & Data Layers ──
builder.Services.AddBusinessLayer(builder.Configuration);
builder.Services.AddScoped<IHotelService, HotelService>();

// ── SignalR Notifiers ──
builder.Services.AddScoped<IBookingHubNotifier, BookingHubNotifier>();
builder.Services.AddScoped<IReviewHubNotifier, ReviewHubNotifier>();
builder.Services.AddScoped<ITicketHubNotifier, TicketHubNotifier>();

// ── SignalR ──
builder.Services.AddSignalR();

// ── Background Services ──
builder.Services.AddHostedService<HotelBooking.Web.Services.BookingExpirationService>();

// ── Razor Pages & Controllers ──
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddHttpClient("vn-provinces");

var app = builder.Build();

// ── Seed Data ──
await SeedData.InitializeAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllers();

// ── SignalR Hub Endpoints ──
app.MapHub<BookingHub>("/hubs/booking");
app.MapHub<ReviewHub>("/hubs/review");
app.MapHub<TicketHub>("/hubs/ticket");

app.Run();
