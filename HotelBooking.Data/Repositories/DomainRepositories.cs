using HotelBooking.Data.Entities;
using HotelBooking.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Data.Repositories;

public class RoomRepository : Repository<Room>, IRoomRepository
{
    public RoomRepository(HotelDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Room>> SearchAsync(
        int? roomTypeId, decimal? minPrice, decimal? maxPrice,
        int? minOccupancy, DateTime? checkIn, DateTime? checkOut,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking()
            .Include(r => r.Hotel)
            .Include(r => r.RoomType)
            .AsQueryable();

        if (roomTypeId.HasValue)
            query = query.Where(r => r.RoomTypeId == roomTypeId.Value);
        if (minPrice.HasValue)
            query = query.Where(r => r.PricePerNight >= minPrice.Value);
        if (maxPrice.HasValue)
            query = query.Where(r => r.PricePerNight <= maxPrice.Value);
        if (minOccupancy.HasValue)
            query = query.Where(r => r.MaxOccupancy >= minOccupancy.Value);
        if (checkIn.HasValue && checkOut.HasValue)
        {
            var ciUtc = DateTime.SpecifyKind(checkIn.Value.Date, DateTimeKind.Utc);
            var coUtc = DateTime.SpecifyKind(checkOut.Value.Date, DateTimeKind.Utc);
            var activeStatuses = new[] { BookingStatus.Pending, BookingStatus.AwaitingPayment, BookingStatus.Confirmed, BookingStatus.CheckedIn, BookingStatus.Completed };
            query = query.Where(r => !r.Bookings.Any(b =>
                activeStatuses.Contains(b.Status) &&
                b.CheckIn < coUtc &&
                b.CheckOut > ciUtc));
        }

        return await query.Where(r => r.IsAvailable).OrderBy(r => r.PricePerNight).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Room>> SearchByHotelAsync(
        int hotelId,
        int? roomTypeId, decimal? minPrice, decimal? maxPrice,
        int? minOccupancy, DateTime? checkIn, DateTime? checkOut,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking()
            .Include(r => r.Hotel)
            .Include(r => r.RoomType)
            .Where(r => r.HotelId == hotelId)
            .AsQueryable();

        if (roomTypeId.HasValue)
            query = query.Where(r => r.RoomTypeId == roomTypeId.Value);
        if (minPrice.HasValue)
            query = query.Where(r => r.PricePerNight >= minPrice.Value);
        if (maxPrice.HasValue)
            query = query.Where(r => r.PricePerNight <= maxPrice.Value);
        if (minOccupancy.HasValue)
            query = query.Where(r => r.MaxOccupancy >= minOccupancy.Value);
        if (checkIn.HasValue && checkOut.HasValue)
        {
            var ciUtc = DateTime.SpecifyKind(checkIn.Value.Date, DateTimeKind.Utc);
            var coUtc = DateTime.SpecifyKind(checkOut.Value.Date, DateTimeKind.Utc);
            var activeStatuses = new[] { BookingStatus.Pending, BookingStatus.AwaitingPayment, BookingStatus.Confirmed, BookingStatus.CheckedIn, BookingStatus.Completed };
            query = query.Where(r => !r.Bookings.Any(b =>
                activeStatuses.Contains(b.Status) &&
                b.CheckIn < coUtc &&
                b.CheckOut > ciUtc));
        }

        return await query.Where(r => r.IsAvailable).OrderBy(r => r.PricePerNight).ToListAsync(ct);
    }

    public async Task<Room?> GetWithDetailsAsync(int id, CancellationToken ct = default)
        => await DbSet.Include(r => r.RoomType)
            .Include(r => r.Reviews.Where(rv => !rv.IsDeleted))
                .ThenInclude(rv => rv.User)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
}

public class RoomTypeRepository : Repository<RoomType>, IRoomTypeRepository
{
    public RoomTypeRepository(HotelDbContext context) : base(context) { }
}

public class BookingRepository : Repository<Booking>, IBookingRepository
{
    public BookingRepository(HotelDbContext context) : base(context) { }

    public async Task<bool> HasOverlappingBookingAsync(int roomId, DateTime checkIn, DateTime checkOut,
        int? excludeBookingId = null, CancellationToken ct = default)
    {
        var ciUtc = DateTime.SpecifyKind(checkIn.Date, DateTimeKind.Utc);
        var coUtc = DateTime.SpecifyKind(checkOut.Date, DateTimeKind.Utc);
        var activeStatuses = new[] { BookingStatus.Pending, BookingStatus.AwaitingPayment, BookingStatus.Confirmed, BookingStatus.CheckedIn, BookingStatus.Completed };
        var query = DbSet.Where(b =>
            b.RoomId == roomId &&
            activeStatuses.Contains(b.Status) &&
            b.CheckIn < coUtc &&
            b.CheckOut > ciUtc);

        if (excludeBookingId.HasValue)
            query = query.Where(b => b.Id != excludeBookingId.Value);

        return await query.AnyAsync(ct);
    }

    public async Task<IReadOnlyList<Booking>> GetByUserAsync(string userId, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(b => b.Room).ThenInclude(r => r.RoomType)
            .Include(b => b.Room).ThenInclude(r => r.Hotel)
            .Include(b => b.Payment)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Booking>> GetByHotelAsync(int hotelId, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(b => b.Room).ThenInclude(r => r.RoomType)
            .Include(b => b.Room).ThenInclude(r => r.Hotel)
            .Include(b => b.User)
            .Include(b => b.Payment)
            .Where(b => b.Room.HotelId == hotelId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Booking>> GetAllWithDetailsAsync(CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(b => b.Room).ThenInclude(r => r.RoomType)
            .Include(b => b.Room).ThenInclude(r => r.Hotel)
            .Include(b => b.User)
            .Include(b => b.Payment)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(ct);

    public async Task<Booking?> GetWithDetailsAsync(int id, CancellationToken ct = default)
        => await DbSet.Include(b => b.Room).ThenInclude(r => r.RoomType)
            .Include(b => b.Room).ThenInclude(r => r.Hotel)
            .Include(b => b.User)
            .Include(b => b.Payment)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

    public async Task<IReadOnlyList<Booking>> GetExpiredPendingAsync(DateTime utcNow, CancellationToken ct = default)
        => await DbSet
            .Where(b => b.Status == BookingStatus.Pending && b.CreatedAt < utcNow.AddHours(-48))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Booking>> GetExpiredAwaitingPaymentAsync(DateTime utcNow, CancellationToken ct = default)
        => await DbSet
            .Where(b => b.Status == BookingStatus.AwaitingPayment && b.PaymentDeadline != null && b.PaymentDeadline < utcNow)
            .ToListAsync(ct);
}

public class HotelRepository : Repository<Hotel>, IHotelRepository
{
    public HotelRepository(HotelDbContext context) : base(context) { }

    public override async Task<IReadOnlyList<Hotel>> GetAllAsync(CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(h => h.Rooms)
            .OrderBy(h => h.Name)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Hotel>> GetActiveHotelsAsync(CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Where(h => h.IsActive)
            .OrderBy(h => h.Name)
            .ToListAsync(ct);

    public async Task<Hotel?> GetWithRoomsAsync(int hotelId, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(h => h.Rooms)
            .FirstOrDefaultAsync(h => h.Id == hotelId, ct);

    public async Task<IReadOnlyList<Hotel>> GetByStaffUserAsync(string userId, CancellationToken ct = default)
        => await Context.Set<HotelStaff>()
            .AsNoTracking()
            .Where(hs => hs.UserId == userId)
            .Include(hs => hs.Hotel)
            .Select(hs => hs.Hotel)
            .OrderBy(h => h.Name)
            .ToListAsync(ct);
}

public class HotelStaffRepository : Repository<HotelStaff>, IHotelStaffRepository
{
    public HotelStaffRepository(HotelDbContext context) : base(context) { }

    public async Task<IReadOnlyList<HotelStaff>> GetByHotelAsync(int hotelId, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(hs => hs.User)
            .Where(hs => hs.HotelId == hotelId)
            .OrderByDescending(hs => hs.AssignedAt)
            .ToListAsync(ct);

    public async Task<HotelStaff?> GetAssignmentAsync(int hotelId, string userId, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(hs => hs.HotelId == hotelId && hs.UserId == userId, ct);

    public async Task<IReadOnlyList<int>> GetHotelIdsByUserAsync(string userId, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Where(hs => hs.UserId == userId)
            .Select(hs => hs.HotelId)
            .ToListAsync(ct);
}

public class ReviewRepository : Repository<Review>, IReviewRepository
{
    public ReviewRepository(HotelDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Review>> GetByRoomWithCommentsAsync(int roomId, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Comments.Where(c => !c.IsDeleted))
                .ThenInclude(c => c.User)
            .Where(r => r.RoomId == roomId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

    public async Task<double> GetAverageRatingAsync(int roomId, CancellationToken ct = default)
    {
        var ratings = await DbSet.Where(r => r.RoomId == roomId).Select(r => r.Rating).ToListAsync(ct);
        return ratings.Count != 0 ? ratings.Average() : 0;
    }

    public async Task<bool> HasUserReviewedRoomAsync(string userId, int roomId, CancellationToken ct = default)
        => await DbSet.AnyAsync(r => r.UserId == userId && r.RoomId == roomId && !r.IsDeleted, ct);
}

public class ReviewCommentRepository : Repository<ReviewComment>, IReviewCommentRepository
{
    public ReviewCommentRepository(HotelDbContext context) : base(context) { }
}

public class TicketRepository : Repository<SupportTicket>, ITicketRepository
{
    public TicketRepository(HotelDbContext context) : base(context) { }

    public async Task<IReadOnlyList<SupportTicket>> GetByUserAsync(string userId, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(t => t.AssignedTo)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<SupportTicket>> GetActiveTicketsAsync(CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.AssignedTo)
            .Where(t => t.Status != TicketStatus.Closed)
            .OrderByDescending(t => t.Priority)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<SupportTicket>> GetAssignedToAsync(string staffId, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(t => t.User)
            .Where(t => t.AssignedToId == staffId && t.Status != TicketStatus.Closed)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);
}

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(HotelDbContext context) : base(context) { }

    public async Task<Payment?> GetByBookingIdAsync(int bookingId, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(p => p.BookingId == bookingId, ct);
}

// ── HR repositories ──
public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(HotelDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Employee>> GetByHotelAsync(int hotelId, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(e => e.Hotel)
            .Where(e => e.HotelId == hotelId)
            .OrderBy(e => e.FullName)
            .ToListAsync(ct);

    public async Task<Employee?> GetByUserIdAsync(string userId, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == userId, ct);
}

public class WorkShiftRepository : Repository<WorkShift>, IWorkShiftRepository
{
    public WorkShiftRepository(HotelDbContext context) : base(context) { }

    public async Task<IReadOnlyList<WorkShift>> GetByHotelAsync(int hotelId, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(ws => ws.Hotel)
            .Where(ws => ws.HotelId == hotelId && ws.IsActive)
            .OrderBy(ws => ws.StartTime)
            .ToListAsync(ct);
}

public class EmployeeShiftAssignmentRepository : Repository<EmployeeShiftAssignment>, IEmployeeShiftAssignmentRepository
{
    public EmployeeShiftAssignmentRepository(HotelDbContext context) : base(context) { }

    public async Task<IReadOnlyList<EmployeeShiftAssignment>> GetByHotelAndDateRangeAsync(
        int hotelId, DateTime start, DateTime end, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(sa => sa.Employee)
            .Include(sa => sa.Hotel)
            .Include(sa => sa.WorkShift)
            .Where(sa => sa.HotelId == hotelId && sa.ShiftDate.Date >= start.Date && sa.ShiftDate.Date <= end.Date)
            .OrderBy(sa => sa.ShiftDate)
            .ThenBy(sa => sa.WorkShift.StartTime)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<EmployeeShiftAssignment>> GetByEmployeeAndDateRangeAsync(
        int employeeId, DateTime start, DateTime end, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(sa => sa.Employee)
            .Include(sa => sa.Hotel)
            .Include(sa => sa.WorkShift)
            .Where(sa => sa.EmployeeId == employeeId && sa.ShiftDate.Date >= start.Date && sa.ShiftDate.Date <= end.Date)
            .OrderBy(sa => sa.ShiftDate)
            .ThenBy(sa => sa.WorkShift.StartTime)
            .ToListAsync(ct);
}

public class AttendanceRepository : Repository<AttendanceRecord>, IAttendanceRepository
{
    public AttendanceRepository(HotelDbContext context) : base(context) { }

    public async Task<IReadOnlyList<AttendanceRecord>> GetByHotelAndDateRangeAsync(
        int hotelId, DateTime start, DateTime end, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(a => a.Employee)
            .Include(a => a.Hotel)
            .Include(a => a.WorkShift)
            .Where(a => a.HotelId == hotelId && a.ShiftDate.Date >= start.Date && a.ShiftDate.Date <= end.Date)
            .OrderBy(a => a.ShiftDate)
            .ThenBy(a => a.Employee.FullName)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<AttendanceRecord>> GetByEmployeeAndDateRangeAsync(
        int employeeId, DateTime start, DateTime end, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(a => a.Employee)
            .Include(a => a.Hotel)
            .Include(a => a.WorkShift)
            .Where(a => a.EmployeeId == employeeId && a.ShiftDate.Date >= start.Date && a.ShiftDate.Date <= end.Date)
            .OrderBy(a => a.ShiftDate)
            .ToListAsync(ct);
}

public class PayrollPeriodRepository : Repository<PayrollPeriod>, IPayrollPeriodRepository
{
    public PayrollPeriodRepository(HotelDbContext context) : base(context) { }

    public async Task<IReadOnlyList<PayrollPeriod>> GetByHotelAsync(int hotelId, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(p => p.Hotel)
            .Where(p => p.HotelId == hotelId)
            .OrderByDescending(p => p.StartDate)
            .ToListAsync(ct);

    public async Task<PayrollPeriod?> GetWithEntriesAsync(int id, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(p => p.Hotel)
            .Include(p => p.Entries)
                .ThenInclude(e => e.Employee)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
}

public class TrainingProgramRepository : Repository<TrainingProgram>, ITrainingProgramRepository
{
    public TrainingProgramRepository(HotelDbContext context) : base(context) { }

    public async Task<IReadOnlyList<TrainingProgram>> GetByHotelOrGlobalAsync(int? hotelId, CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking()
            .Include(tp => tp.Hotel)
            .AsQueryable();

        if (hotelId.HasValue)
        {
            query = query.Where(tp => tp.HotelId == null || tp.HotelId == hotelId.Value);
        }

        return await query.OrderBy(tp => tp.StartDate).ToListAsync(ct);
    }
}

public class TrainingEnrollmentRepository : Repository<TrainingEnrollment>, ITrainingEnrollmentRepository
{
    public TrainingEnrollmentRepository(HotelDbContext context) : base(context) { }

    public async Task<IReadOnlyList<TrainingEnrollment>> GetByEmployeeAsync(int employeeId, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(e => e.TrainingProgram)
            .Include(e => e.Employee)
            .Where(e => e.EmployeeId == employeeId)
            .OrderBy(e => e.TrainingProgram.StartDate)
            .ToListAsync(ct);
}

public class PerformanceReviewRepository : Repository<PerformanceReview>, IPerformanceReviewRepository
{
    public PerformanceReviewRepository(HotelDbContext context) : base(context) { }

    public async Task<IReadOnlyList<PerformanceReview>> GetByEmployeeAsync(int employeeId, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(r => r.Employee)
            .Include(r => r.Hotel)
            .Include(r => r.Reviewer)
            .Where(r => r.EmployeeId == employeeId)
            .OrderByDescending(r => r.ReviewDate)
            .ToListAsync(ct);
}

public class EmploymentContractRepository : Repository<EmploymentContract>, IEmploymentContractRepository
{
    public EmploymentContractRepository(HotelDbContext context) : base(context) { }

    public async Task<IReadOnlyList<EmploymentContract>> GetByEmployeeAsync(int employeeId, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(c => c.Employee)
            .Include(c => c.Hotel)
            .Where(c => c.EmployeeId == employeeId)
            .OrderByDescending(c => c.StartDate)
            .ToListAsync(ct);
}

public class InsuranceRecordRepository : Repository<InsuranceRecord>, IInsuranceRecordRepository
{
    public InsuranceRecordRepository(HotelDbContext context) : base(context) { }

    public async Task<IReadOnlyList<InsuranceRecord>> GetByEmployeeAsync(int employeeId, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(i => i.Employee)
            .Where(i => i.EmployeeId == employeeId)
            .OrderByDescending(i => i.EffectiveDate)
            .ToListAsync(ct);
}
