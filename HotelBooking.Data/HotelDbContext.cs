using HotelBooking.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Data;

public class HotelDbContext : IdentityDbContext<ApplicationUser>
{
    public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options) { }

    public DbSet<Hotel> Hotels => Set<Hotel>();
    public DbSet<HotelStaff> HotelStaff => Set<HotelStaff>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<RoomType> RoomTypes => Set<RoomType>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ReviewComment> ReviewComments => Set<ReviewComment>();
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<WorkShift> WorkShifts => Set<WorkShift>();
    public DbSet<EmployeeShiftAssignment> EmployeeShiftAssignments => Set<EmployeeShiftAssignment>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<PayrollPeriod> PayrollPeriods => Set<PayrollPeriod>();
    public DbSet<PayrollEntry> PayrollEntries => Set<PayrollEntry>();
    public DbSet<TrainingProgram> TrainingPrograms => Set<TrainingProgram>();
    public DbSet<TrainingEnrollment> TrainingEnrollments => Set<TrainingEnrollment>();
    public DbSet<PerformanceReview> PerformanceReviews => Set<PerformanceReview>();
    public DbSet<EmploymentContract> EmploymentContracts => Set<EmploymentContract>();
    public DbSet<InsuranceRecord> InsuranceRecords => Set<InsuranceRecord>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(HotelDbContext).Assembly);
    }
}
