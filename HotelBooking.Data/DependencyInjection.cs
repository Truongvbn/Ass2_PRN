using HotelBooking.Data.Entities;
using HotelBooking.Data.Repositories;
using HotelBooking.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HotelBooking.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddDataLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<HotelDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.SignIn.RequireConfirmedAccount = false;
        })
        .AddEntityFrameworkStores<HotelDbContext>()
        .AddDefaultTokenProviders();

        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IRoomTypeRepository, RoomTypeRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IHotelRepository, HotelRepository>();
        services.AddScoped<IHotelStaffRepository, HotelStaffRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IReviewCommentRepository, ReviewCommentRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();

        // ── HR repositories ──
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IWorkShiftRepository, WorkShiftRepository>();
        services.AddScoped<IEmployeeShiftAssignmentRepository, EmployeeShiftAssignmentRepository>();
        services.AddScoped<IAttendanceRepository, AttendanceRepository>();
        services.AddScoped<IPayrollPeriodRepository, PayrollPeriodRepository>();
        services.AddScoped<ITrainingProgramRepository, TrainingProgramRepository>();
        services.AddScoped<ITrainingEnrollmentRepository, TrainingEnrollmentRepository>();
        services.AddScoped<IPerformanceReviewRepository, PerformanceReviewRepository>();
        services.AddScoped<IEmploymentContractRepository, EmploymentContractRepository>();
        services.AddScoped<IInsuranceRecordRepository, InsuranceRecordRepository>();

        return services;
    }
}
