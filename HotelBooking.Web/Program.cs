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

// ── Data Layer ──
builder.Services.AddDbContext<HotelDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Identity ──
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<HotelDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
});

// ── Repositories (Data Layer) ──
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IRoomTypeRepository, RoomTypeRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IReviewCommentRepository, ReviewCommentRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

// ── AutoMapper ──
builder.Services.AddAutoMapper(typeof(MappingProfile));

// ── Services (Business Layer) ──
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IAiAssistantService, MockAiAssistantService>();

// ── SignalR Notifiers ──
builder.Services.AddScoped<IBookingHubNotifier, BookingHubNotifier>();
builder.Services.AddScoped<IReviewHubNotifier, ReviewHubNotifier>();
builder.Services.AddScoped<ITicketHubNotifier, TicketHubNotifier>();

// ── SignalR ──
builder.Services.AddSignalR();

// ── Razor Pages ──
builder.Services.AddRazorPages();

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

// ── SignalR Hub Endpoints ──
app.MapHub<BookingHub>("/hubs/booking");
app.MapHub<ReviewHub>("/hubs/review");
app.MapHub<TicketHub>("/hubs/ticket");

app.Run();
